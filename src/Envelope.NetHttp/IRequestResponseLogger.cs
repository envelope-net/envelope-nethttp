using Envelope.NetHttp.Http;
using Envelope.Trace;
using Envelope.Web.Logging;

namespace Envelope.NetHttp;

public interface IRequestResponseLogger
{
}

public interface IRequestResponseLogger<T> : IRequestResponseLogger
{
	Task<T> LogRequestAsync<TOptions>(
		RequestDto request,
		HttpContentDto requestContent,
		ITraceInfo traceInfo,
		IServiceProvider serviceProvider,
		TOptions options,
		CancellationToken cancellationToken = default)
		where TOptions : HttpApiClientOptions;

	Task LogResponseAsync<TOptions>(
		T requestIdentifier,
		ResponseDto response,
		HttpContentDto responseContent,
		ITraceInfo traceInfo,
		IServiceProvider serviceProvider,
		TOptions options,
		CancellationToken cancellationToken = default)
		where TOptions : HttpApiClientOptions;
}
