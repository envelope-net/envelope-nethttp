//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using Envelope.Logging.Extensions;
//using Envelope.NetHttp.Http;
//using System.Diagnostics;
//using Microsoft.Extensions.DependencyInjection;

//namespace Envelope.NetHttp;

///// <inheritdoc />
//internal class LogHandler<TOptions, TIdentity> : DelegatingHandler
//	where TOptions : HttpApiClientOptions
//	where TIdentity : struct
//{
//	private readonly TOptions _options;
//	private readonly IServiceProvider _serviceProvider;
//	private readonly ILogger _errorLogger;

//	public LogHandler(IOptions<TOptions> options, IServiceProvider serviceProvider, ILogger<LogHandler<TOptions, TIdentity>> errorLogger)
//	{
//		_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
//		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
//		_errorLogger = errorLogger ?? throw new ArgumentNullException(nameof(errorLogger));
//	}

//	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
//	{
//		var uri = request.RequestUri?.ToString();
//		var correlationId = Guid.NewGuid();
//		Stopwatch? sw = null;

//		var logger = _options.GetLogger(uri, _serviceProvider);
//		if (logger != null)
//		{
//			var requestDto = await RequestDtoMapper.MapAsync(request, null, null, null, true, false, false, cancellationToken).ConfigureAwait(false);
//			var httpContentDto = await HttpContentHelper.ParseHttpContentAsync(request.Content);

//			try
//			{
//				await logger.LogRequestAsync(requestDto, httpContentDto, correlationId, cancellationToken).ConfigureAwait(false);
//			}
//			catch (Exception ex)
//			{
//				var applicationCoxntext = _serviceProvider.GetService<IApplicationContext<TIdentity>>();
//				if (applicationCoxntext != null)
//					_errorLogger.LogErrorMessage<TIdentity>(applicationCoxntext, x => x.ExceptionInfo(ex).Detail($"{_options.SourceSystemName}: {nameof(LogHandler<TOptions, TIdentity>)}.{nameof(SendAsync)} - {nameof(logger.LogRequestAsync)}"), true);
//				else
//					_errorLogger.LogErrorMessage<TIdentity>(_options.SourceSystemName, x => x.ExceptionInfo(ex).Detail($"{nameof(LogHandler<TOptions, TIdentity>)}.{nameof(SendAsync)} - {nameof(logger.LogRequestAsync)}"), true);
//			}

//			sw = Stopwatch.StartNew();
//		}

//		var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

//		if (logger != null)
//		{
//			sw?.Stop();

//			var responseDto = await ResponseDtoMapper.MapAsync(response, null, null, sw?.ElapsedMilliseconds, true, false, false, cancellationToken).ConfigureAwait(false);
//			var httpContentDto = await HttpContentHelper.ParseHttpContentAsync(response.Content);

//			try
//			{
//				await logger.LogResponseAsync(responseDto, httpContentDto, correlationId, cancellationToken).ConfigureAwait(false);
//			}
//			catch (Exception ex)
//			{
//				var applicationCoxntext = _serviceProvider.GetService<IApplicationContext<TIdentity>>();
//				if (applicationCoxntext != null)
//					_errorLogger.LogErrorMessage<TIdentity>(applicationCoxntext, x => x.ExceptionInfo(ex).Detail($"{_options.SourceSystemName}: {nameof(LogHandler<TOptions, TIdentity>)}.{nameof(SendAsync)} - {nameof(logger.LogResponseAsync)}"), true);
//				else
//					_errorLogger.LogErrorMessage<TIdentity>(_options.SourceSystemName, x => x.ExceptionInfo(ex).Detail($"{nameof(LogHandler<TOptions, TIdentity>)}.{nameof(SendAsync)} - {nameof(logger.LogResponseAsync)}"), true);
//			}
//		}

//		return response;
//	}
//}
