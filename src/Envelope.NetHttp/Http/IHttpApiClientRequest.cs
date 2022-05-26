using Envelope.NetHttp.Http.Headers;

namespace Envelope.NetHttp.Http;

public interface IHttpApiClientRequest
{
	string? BaseAddress { get; set; }
	string? RelativePath { get; set; }
	string? QueryString { get; set; }
	string? HttpMethod { get; set; }
	bool ClearDefaultHeaders { get; set; }
	RequestHeaders Headers { get; }
	string? MultipartSubType { get; set; }
	string? MultipartBoundary { get; set; }
	TimeSpan? RequestTimeout { get; set; }
	List<KeyValuePair<string, string>>? FormData { get; set; }
	List<StringContent>? StringContents { get; set; }
	List<JsonContent>? JsonContents { get; set; }
	List<StreamContent>? StreamContents { get; set; }
	List<ByteArrayContent>? ByteArrayContents { get; set; }

	HttpRequestMessage ToHttpRequestMessage();
	string? GetRequestUri();
	HttpContent? ToHttpContent();
}
