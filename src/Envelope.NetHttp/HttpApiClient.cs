using Envelope.Exceptions;
using Envelope.Extensions;
using Envelope.Logging;
using Envelope.Logging.Extensions;
using Envelope.NetHttp.Http;
using Envelope.Trace;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Text;

namespace Envelope.NetHttp;

public abstract class HttpApiClient
{
	private readonly HttpClient _client;
#if NET6_0_OR_GREATER
	private readonly HttpRequestOptionsKey<IServiceProvider> _serviceProviderHttpRequestOptionsKey;
	private readonly HttpRequestOptionsKey<ITraceInfo> _traceInfoHttpRequestOptionsKey;
#endif
	protected IServiceProvider ServiceProvider { get; }

	protected HttpApiClientOptions Options { get; }
	protected ILogger Logger { get; }

	public HttpApiClient(
		HttpClient client,
		IServiceProvider serviceProvider,
		IOptions<HttpApiClientOptions> options,
		ILogger<HttpApiClient> logger)
	{
		_client = client ?? throw new ArgumentNullException(nameof(client));
		ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		Options = options?.Value ?? throw new ArgumentNullException(nameof(options));
		Logger = logger ?? throw new ArgumentNullException(nameof(logger));

		var error = Options.Validate();
		if (0 < error?.Count)
			throw new ConfigurationException(error);

#if NET6_0_OR_GREATER
		_serviceProviderHttpRequestOptionsKey = new(nameof(IServiceProvider));
		_traceInfoHttpRequestOptionsKey = new(nameof(ITraceInfo));
#endif
	}

	public Task<IHttpApiClientResponse> SendAsync(Action<RequestBuilder> configureRequest, ITraceInfo traceInfo, IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default)
	{
		var builder = new RequestBuilder();
		configureRequest.Invoke(builder);

		return SendAsync(builder.Build(), traceInfo, serviceProvider, false, cancellationToken);
	}

	public Task<IHttpApiClientResponse> SendAsync(Action<RequestBuilder> configureRequest, ITraceInfo traceInfo, IServiceProvider? serviceProvider, bool? continueOnCapturedContext, CancellationToken cancellationToken = default)
	{
		var builder = new RequestBuilder();
		configureRequest.Invoke(builder);

		return SendAsync(builder.Build(), traceInfo, serviceProvider, continueOnCapturedContext, cancellationToken);
	}

	public Task<IHttpApiClientResponse> SendAsync(HttpRequestMessage request, ITraceInfo traceInfo, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
		=> SendAsync(HttpApiClientRequest.FromHttpRequest(request, false), traceInfo, serviceProvider, false, cancellationToken);

	public Task<IHttpApiClientResponse> SendAsync(IHttpApiClientRequest request, ITraceInfo traceInfo, IServiceProvider? serviceProvider, CancellationToken cancellationToken = default)
		=> SendAsync(request, traceInfo, serviceProvider, false, cancellationToken);

	public async Task<IHttpApiClientResponse> SendAsync(
		IHttpApiClientRequest request,
		ITraceInfo traceInfo,
		IServiceProvider? serviceProvider,
		bool? continueOnCapturedContext,
		CancellationToken cancellationToken = default)
	{
		if (request == null)
			throw new ArgumentNullException(nameof(request));
		if (traceInfo == null)
			throw new ArgumentNullException(nameof(traceInfo));

		if (string.IsNullOrWhiteSpace(request.BaseAddress))
			request.BaseAddress = Options.BaseAddress;

		Options.ConfigureStaticRequestParams(request);

		var response = new Http.Internal.HttpApiClientResponse(request);

		CancellationTokenSource? requestTimeoutCancellationTokenSource = null;
		CancellationTokenSource? linkedCancellationTokenSource = null;
		var usedCancellationToken = cancellationToken;
		try
		{
			if (request.RequestTimeout.HasValue)
			{
				requestTimeoutCancellationTokenSource = new CancellationTokenSource(request.RequestTimeout.Value);
				if (cancellationToken != default)
				{
					linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(requestTimeoutCancellationTokenSource.Token, cancellationToken);
					usedCancellationToken = linkedCancellationTokenSource.Token;
				}
				else
				{
					usedCancellationToken = requestTimeoutCancellationTokenSource.Token;
				}
			}

			using var httpRequestMessage = request.ToHttpRequestMessage();
#if NET6_0_OR_GREATER
			httpRequestMessage.Options.Set(_serviceProviderHttpRequestOptionsKey, serviceProvider ?? ServiceProvider);
			httpRequestMessage.Options.Set(_traceInfoHttpRequestOptionsKey, traceInfo);
#elif NETSTANDARD2_0 || NETSTANDARD2_1
			httpRequestMessage.Properties.Add(nameof(IServiceProvider), serviceProvider ?? ServiceProvider);
			httpRequestMessage.Properties.Add(nameof(ITraceInfo), traceInfo);
#endif

			if (continueOnCapturedContext.HasValue)
			{
				var httpResponseMessageTask =
					_client
						.SendAsync(httpRequestMessage, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, usedCancellationToken)
						.ConfigureAwait(continueOnCapturedContext: continueOnCapturedContext.Value);

				response.HttpResponseMessage = await httpResponseMessageTask;
			}
			else
			{
				var httpResponseMessageTask =
					_client
						.SendAsync(httpRequestMessage, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, usedCancellationToken);

				response.HttpResponseMessage = await httpResponseMessageTask;
			}

			try
			{
#if NET6_0_OR_GREATER
				httpRequestMessage.Options.RemoveIfKeyExists(nameof(IServiceProvider));
				httpRequestMessage.Options.RemoveIfKeyExists(nameof(ITraceInfo));
#elif NETSTANDARD2_0 || NETSTANDARD2_1
			httpRequestMessage.Properties.RemoveIfKeyExists(nameof(IServiceProvider));
			httpRequestMessage.Properties.RemoveIfKeyExists(nameof(ITraceInfo));
#endif
			}
			catch { }
		}
		catch (TaskCanceledException)
		{
			if (requestTimeoutCancellationTokenSource != null && requestTimeoutCancellationTokenSource.IsCancellationRequested)
				response.RequestTimedOut = true;
			else
				response.OperationCanceled = true;
		}
		catch (TimeoutException)
		{
			response.RequestTimedOut = true;
		}
		catch (OperationCanceledException)
		{
			response.OperationCanceled = true;
		}
		catch (Exception ex)
		{
			response.Exception = ex;
		}
		finally
		{
			linkedCancellationTokenSource?.Dispose();
			requestTimeoutCancellationTokenSource?.Dispose();
		}

		return response;
	}

	protected virtual ErrorMessageBuilder? CreateErrorMessage(
		ITraceInfo traceInfo,
		IHttpApiClientRequest? request,
		IHttpApiClientResponse? response)
	{
		if (request == null && response == null)
			return null;

		traceInfo = TraceInfo.Create(traceInfo);
		ErrorMessageBuilder? builder;

		if (response == null)
		{
			builder = new ErrorMessageBuilder(traceInfo);
			builder.InternalMessage("NO RESPONSE").Detail($"URI = {request?.GetRequestUri()}");
		}
		else
		{
			response.HasErrorOrNoResponse(traceInfo, out builder);
		}

		return builder;
	}

	protected virtual ErrorMessageBuilder? CreateErrorMessage(
		IApplicationContext applicationContext,
		IHttpApiClientRequest? request,
		IHttpApiClientResponse? response,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
	{
		if (request == null && response == null)
			return null;

		var traceInfo = TraceInfo.Create(applicationContext, null, memberName, sourceFilePath, sourceLineNumber);
		ErrorMessageBuilder? builder;

		if (response == null)
		{
			builder = new ErrorMessageBuilder(traceInfo);
			builder.InternalMessage("NO RESPONSE").Detail($"URI = {request?.GetRequestUri()}");
		}
		else
		{
			response.HasErrorOrNoResponse(traceInfo, out builder);
		}

		return builder;
	}

	//protected virtual ErrorMessageBuilder<TIdentity>? CreateErrorMessage<TIdentity>(
	//	string sourceSystemName,
	//	IHttpApiClientRequest? request,
	//	IHttpApiClientResponse? response,
	//	[CallerMemberName] string memberName = "",
	//	[CallerFilePath] string sourceFilePath = "",
	//	[CallerLineNumber] int sourceLineNumber = 0)
	//	where TIdentity : struct
	//{
	//	if (request == null && response == null)
	//		return null;

	//	var traceInfo = TraceInfo<TIdentity>.Create(null, sourceSystemName, null, memberName, sourceFilePath, sourceLineNumber);
	//	ErrorMessageBuilder<TIdentity>? builder;

	//	if (response == null)
	//	{
	//		builder = new ErrorMessageBuilder<TIdentity>(traceInfo);
	//		builder.InternalMessage("NO RESPONSE").Detail($"URI = {request?.GetRequestUri()}");
	//	}
	//	else
	//	{
	//		response.HasErrorOrNoResponse(traceInfo, out builder);
	//	}

	//	return builder;
	//}

	protected virtual ErrorMessageBuilder? LogError(
		IApplicationContext applicationContext,
		IHttpApiClientRequest? request,
		IHttpApiClientResponse? response,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
	{
		if (request == null && response == null)
			return null;

		var errorMessageBuilder = CreateErrorMessage(applicationContext, request, response, memberName, sourceFilePath, sourceLineNumber);

		if (errorMessageBuilder == null)
			return null;

		Logger.LogErrorMessage(errorMessageBuilder.Build(), true);

		return errorMessageBuilder;
	}

	protected virtual ErrorMessageBuilder? LogError(
		ITraceInfo traceInfo,
		IHttpApiClientRequest? request,
		IHttpApiClientResponse? response)
	{
		if (request == null && response == null)
			return null;

		var errorMessageBuilder = CreateErrorMessage(traceInfo, request, response);

		if (errorMessageBuilder == null)
			return null;

		Logger.LogErrorMessage(errorMessageBuilder.Build(), true);

		return errorMessageBuilder;
	}

	//protected virtual ErrorMessageBuilder<TIdentity>? LogError<TIdentity>(
	//	string sourceSystemName,
	//	IHttpApiClientRequest? request,
	//	IHttpApiClientResponse? response,
	//	[CallerMemberName] string memberName = "",
	//	[CallerFilePath] string sourceFilePath = "",
	//	[CallerLineNumber] int sourceLineNumber = 0)
	//	where TIdentity : struct
	//{
	//	if (request == null && response == null)
	//		return null;

	//	var errorMessageBuilder = CreateErrorMessage<TIdentity>(sourceSystemName, request, response, memberName, sourceFilePath, sourceLineNumber);

	//	if (errorMessageBuilder == null)
	//		return null;

	//	Logger.LogErrorMessage(errorMessageBuilder.Build(), true);

	//	return errorMessageBuilder;
	//}

	protected virtual StringBuilder LogErrorToStringBuilder(
		IHttpApiClientRequest? request,
		IHttpApiClientResponse? response,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
	{
		var sb = new StringBuilder();

		if (request != null)
			sb.AppendLine($"URI = {request.GetRequestUri()}");

		if (response != null)
		{
			if (request == null && response.Request != null)
				sb.AppendLine($"URI = {response.Request.GetRequestUri()}");

			sb.AppendLine($"{nameof(response.StatusCode)} = {response.StatusCode}");

			if (response.OperationCanceled.HasValue)
				sb.AppendLine($"{nameof(response.OperationCanceled)} = {response.OperationCanceled}");

			if (response.RequestTimedOut.HasValue)
				sb.AppendLine($"{nameof(response.RequestTimedOut)} = {response.RequestTimedOut}");

			if (response.Exception != null)
				sb.AppendLine($"Exception: {response.Exception.ToStringTrace()}");
		}
		else
		{
			sb.AppendLine("NO RESPONSE");
		}

		return sb;
	}
}
