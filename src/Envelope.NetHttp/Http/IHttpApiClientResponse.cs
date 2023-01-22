using Envelope.Logging;
using Envelope.Trace;
using System.Diagnostics.CodeAnalysis;

namespace Envelope.NetHttp.Http;

#if NET6_0_OR_GREATER
[Envelope.Serializer.JsonPolymorphicConverter]
#endif
public interface IHttpApiClientResponse : IDisposable
{
	IHttpApiClientRequest Request { get; }
	HttpResponseMessage? HttpResponseMessage { get; }
	int? StatusCode { get; }
	bool? RequestTimedOut { get; }
	bool? OperationCanceled { get; }

#if NETSTANDARD2_0 || NETSTANDARD2_1
	[Newtonsoft.Json.JsonIgnore]
#elif NET6_0_OR_GREATER
	[System.Text.Json.Serialization.JsonIgnore]
#endif
	Exception? Exception { get; }
	string? CancelOrTimeoutExceptionText { get; }
	string? ExceptionText { get; }

#if NETSTANDARD2_0 || NETSTANDARD2_1
	[Newtonsoft.Json.JsonIgnore]
#elif NET6_0_OR_GREATER
	[System.Text.Json.Serialization.JsonIgnore]
#endif
	bool StatusCodeIsOK { get; }

#if NETSTANDARD2_0 || NETSTANDARD2_1
	[Newtonsoft.Json.JsonIgnore]
#elif NET6_0_OR_GREATER
	[System.Text.Json.Serialization.JsonIgnore]
#endif
	bool IsOK { get; }

	ErrorMessageBuilder? GetErrorMessageBuilder(ITraceInfo traceInfo, bool checkResponseNotNull);

	Action<ErrorMessageBuilder>? GetErrorMessageBuilderAction(bool checkResponseNotNull);

	bool HasError(bool checkResponseNotNull);

	bool HasError(ITraceInfo traceInfo, [MaybeNullWhen(false)] out ErrorMessageBuilder errorMessageBuilder);

	bool HasErrorOrNoResponse(ITraceInfo traceInfo, [MaybeNullWhen(false)] out ErrorMessageBuilder errorMessageBuilder);

	bool HasError([MaybeNullWhen(false)] out Action<ErrorMessageBuilder> errorMessageBuilder);

	bool HasErrorOrNoResponse([MaybeNullWhen(false)] out Action<ErrorMessageBuilder> errorMessageBuilder);

	List<KeyValuePair<string, IEnumerable<string>>>? GetAllHeaders();
	List<KeyValuePair<string, IEnumerable<string>>>? GetResponseHeaders();
	List<KeyValuePair<string, IEnumerable<string>>>? GetContentHeaders();

	Task CopyContentToAsync(Stream stream, CancellationToken cancellationToken);
	Task<Stream?> ReadContentAsStreamAsync(CancellationToken cancellationToken);
	Task<byte[]?> ReadContentAsByteArrayAsync(CancellationToken cancellationToken);
	Task<string?> ReadContentAsStringAsync(CancellationToken cancellationToken);
#if NETSTANDARD2_0 || NETSTANDARD2_1
	Task<T?> ReadJsonContentAsAsync<T>(Newtonsoft.Json.JsonSerializerSettings? jsonSerializerOptions = null, CancellationToken cancellationToken = default);
#elif NET6_0_OR_GREATER
	Task<T?> ReadJsonContentAsAsync<T>(System.Text.Json.JsonSerializerOptions? jsonSerializerOptions = null, CancellationToken cancellationToken = default);
#endif
}
