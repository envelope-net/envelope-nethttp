using System.Net.Http.Headers;

namespace Envelope.NetHttp.Http.Headers;

public class ContentHeaders
{
	public List<string>? Allow { get; set; }
	public ContentDispositionHeader? ContentDisposition { get; set; }
	public List<string>? ContentEncoding { get; set; }
	public List<string>? ContentLanguage { get; set; }
	public long? ContentLength { get; set; }
	public string? ContentLocation { get; set; }
	public byte[]? ContentMD5 { get; set; }
	public ContentRangeHeader? ContentRange { get; set; }
	public MediaTypeHeader? ContentType { get; set; }
	public DateTimeOffset? Expires { get; set; }
	public DateTimeOffset? LastModified { get; set; }

	public void SetHttpContentHeaders(HttpContentHeaders httpContentHeaders)
	{
		if (httpContentHeaders == null)
			throw new ArgumentNullException(nameof(httpContentHeaders));

		if (Allow != null && 0 < Allow.Count)
			foreach (var allow in Allow)
				httpContentHeaders.Allow.Add(allow);

		if (ContentDisposition != null)
			httpContentHeaders.ContentDisposition = ContentDisposition.ToContentDispositionHeaderValue();

		if (ContentEncoding != null && 0 < ContentEncoding.Count)
			foreach (var contentEncoding in ContentEncoding)
				httpContentHeaders.ContentEncoding.Add(contentEncoding);

		if (ContentLanguage != null && 0 < ContentLanguage.Count)
			foreach (var contentLanguage in ContentLanguage)
				httpContentHeaders.ContentLanguage.Add(contentLanguage);

		if (ContentLength.HasValue)
			httpContentHeaders.ContentLength = ContentLength;

		if (ContentLocation != null)
			httpContentHeaders.ContentLocation = new Uri(ContentLocation);

		if (ContentMD5 != null)
			httpContentHeaders.ContentMD5 = ContentMD5;

		if (ContentRange != null)
			httpContentHeaders.ContentRange = ContentRange.ToContentRangeHeaderValue();

		if (ContentType != null)
			httpContentHeaders.ContentType = ContentType.ToMediaTypeHeaderValue();

		if (Expires.HasValue)
			httpContentHeaders.Expires = Expires;

		if (LastModified.HasValue)
			httpContentHeaders.LastModified = LastModified;
	}
}
