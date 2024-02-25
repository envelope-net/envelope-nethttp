using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Envelope.Logging.Extensions;
using Envelope.NetHttp.Http;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Envelope.Extensions;
using Envelope.Trace;

namespace Envelope.NetHttp;

/// <inheritdoc />
internal class LogHandler<TOptions, TCorrelation> : DelegatingHandler
	where TOptions : HttpApiClientOptions
{
#if NET6_0_OR_GREATER
	private readonly HttpRequestOptionsKey<IServiceProvider> _serviceProviderHttpRequestOptionsKey;
	private readonly HttpRequestOptionsKey<ITraceInfo> _traceInfoHttpRequestOptionsKey;
#endif

	private readonly TOptions _options;
	private readonly ILogger _errorLogger;

	public LogHandler(IOptions<TOptions> options, IServiceProvider serviceProvider, ILogger<LogHandler<TOptions, TCorrelation>> errorLogger)
	{
		_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
		_errorLogger = errorLogger ?? throw new ArgumentNullException(nameof(errorLogger));

#if NET6_0_OR_GREATER
		_serviceProviderHttpRequestOptionsKey = new(nameof(IServiceProvider));
		_traceInfoHttpRequestOptionsKey = new(nameof(ITraceInfo));
#endif
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var uri = request.RequestUri?.ToString();
		Stopwatch? sw = null;

#if NET6_0_OR_GREATER
		if (!request.Options.TryGetValue(_serviceProviderHttpRequestOptionsKey, out IServiceProvider? serviceProvider))
		{
			var exception = $"{nameof(serviceProvider)} == null";
			_errorLogger.LogErrorMessage(
				_options.SourceSystemName,
				x => x.InternalMessage(exception).Detail($"{nameof(LogHandler<TOptions, TCorrelation>)}.{nameof(SendAsync)}"),
				true);

			throw new InvalidOperationException(exception);
		}

		if (!request.Options.TryGetValue(_traceInfoHttpRequestOptionsKey, out ITraceInfo? traceInfo))
		{
			var exception = $"{nameof(traceInfo)} == null";
			_errorLogger.LogErrorMessage(
				_options.SourceSystemName,
				x => x.InternalMessage(exception).Detail($"{nameof(LogHandler<TOptions, TCorrelation>)}.{nameof(SendAsync)}"),
				true);

			throw new InvalidOperationException(exception);
		}
#elif NETSTANDARD2_0 || NETSTANDARD2_1
		if (!request.Properties.TryGetValue(nameof(IServiceProvider), out IServiceProvider? serviceProvider))
		{
			var exception = $"{nameof(serviceProvider)} == null";
			_errorLogger.LogErrorMessage(
				_options.SourceSystemName,
				x => x.InternalMessage(exception).Detail($"{nameof(LogHandler<TOptions, TCorrelation>)}.{nameof(SendAsync)}"),
				true);

			throw new InvalidOperationException(exception);
		}
		
		if (!request.Properties.TryGetValue(nameof(ITraceInfo), out ITraceInfo? traceInfo))
		{
			var exception = $"{nameof(traceInfo)} == null";
			_errorLogger.LogErrorMessage(
				_options.SourceSystemName,
				x => x.InternalMessage(exception).Detail($"{nameof(LogHandler<TOptions, TCorrelation>)}.{nameof(SendAsync)}"),
				true);

			throw new InvalidOperationException(exception);
		}
#endif

		traceInfo = TraceInfo.Create(traceInfo);

		TCorrelation requestLogIdentifier = default;
		var logger = _options.GetLogger<TCorrelation>(uri, serviceProvider);
		if (logger != null)
		{
			var requestDto = await RequestDtoMapper.MapAsync(request, null, null, null, true, false, false, cancellationToken).ConfigureAwait(false);
			var httpContentDto = HttpContentHelper.ParseHttpContent(request.Content);

			try
			{
				requestLogIdentifier = await logger.LogRequestAsync(
					requestDto,
					httpContentDto,
					traceInfo,
					serviceProvider,
					_options,
					cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				var applicationCoxntext = serviceProvider.GetService<IApplicationContext>();
				if (applicationCoxntext != null)
					_errorLogger.LogErrorMessage(applicationCoxntext, x => x.ExceptionInfo(ex).Detail($"{_options.SourceSystemName}: {nameof(LogHandler<TOptions, TCorrelation>)}.{nameof(SendAsync)} - {nameof(logger.LogRequestAsync)}"), true);
				else
					_errorLogger.LogErrorMessage(_options.SourceSystemName, x => x.ExceptionInfo(ex).Detail($"{nameof(LogHandler<TOptions, TCorrelation>)}.{nameof(SendAsync)} - {nameof(logger.LogRequestAsync)}"), true);
			}

			sw = Stopwatch.StartNew();
		}

		HttpResponseMessage? response = null;
		string? error = null;
		try
		{
			response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

			if (logger != null && requestLogIdentifier != null)
			{
				sw?.Stop();

				var responseDto = await ResponseDtoMapper.MapAsync(response, null, null, error, sw?.ElapsedMilliseconds, true, false, false, cancellationToken).ConfigureAwait(false);
				var httpContentDto = HttpContentHelper.ParseHttpContent(response?.Content);

				try
				{
					await logger.LogResponseAsync(
						requestLogIdentifier,
						responseDto,
						httpContentDto,
						traceInfo,
						serviceProvider,
						_options,
						cancellationToken).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					var applicationCoxntext = serviceProvider.GetService<IApplicationContext>();
					if (applicationCoxntext != null)
						_errorLogger.LogErrorMessage(applicationCoxntext, x => x.ExceptionInfo(ex).Detail($"{_options.SourceSystemName}: {nameof(LogHandler<TOptions, TCorrelation>)}.{nameof(SendAsync)} - {nameof(logger.LogResponseAsync)}"), true);
					else
						_errorLogger.LogErrorMessage(_options.SourceSystemName, x => x.ExceptionInfo(ex).Detail($"{nameof(LogHandler<TOptions, TCorrelation>)}.{nameof(SendAsync)} - {nameof(logger.LogResponseAsync)}"), true);
				}
			}

			return response!;
		}
		catch (Exception ex)
		{
			error = ex.ToStringTrace();

			if (logger != null && requestLogIdentifier != null)
			{
				sw?.Stop();

				var responseDto = await ResponseDtoMapper.MapAsync(response, null, null, error, sw?.ElapsedMilliseconds, true, false, false, cancellationToken).ConfigureAwait(false);
				var httpContentDto = HttpContentHelper.ParseHttpContent(response?.Content);

				try
				{
					await logger.LogResponseAsync(
						requestLogIdentifier,
						responseDto,
						httpContentDto,
						traceInfo,
						serviceProvider,
						_options,
						cancellationToken).ConfigureAwait(false);
				}
				catch (Exception exLog)
				{
					var applicationCoxntext = serviceProvider.GetService<IApplicationContext>();
					if (applicationCoxntext != null)
						_errorLogger.LogErrorMessage(applicationCoxntext, x => x.ExceptionInfo(exLog).Detail($"{_options.SourceSystemName}: {nameof(LogHandler<TOptions, TCorrelation>)}.{nameof(SendAsync)} - {nameof(logger.LogResponseAsync)}"), true);
					else
						_errorLogger.LogErrorMessage(_options.SourceSystemName, x => x.ExceptionInfo(exLog).Detail($"{nameof(LogHandler<TOptions, TCorrelation>)}.{nameof(SendAsync)} - {nameof(logger.LogResponseAsync)}"), true);
				}
			}

			throw;
		}
	}
}
