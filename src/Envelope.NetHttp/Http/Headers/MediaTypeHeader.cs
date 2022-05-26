using System.Net.Http.Headers;
using System.Text;

namespace Envelope.NetHttp.Http.Headers;

public class MediaTypeHeader
{
	internal static readonly Encoding DefaultStringEncoding = Encoding.UTF8;

	public string? MediaType { get; set; }

	/// <summary>
	/// equals to Encoding.WebName
	/// </summary>
	public string? CharSet { get; set; }

	public List<NameValueHeader>? Parameters { get; set; }

	public MediaTypeHeader()
	{
	}

	public MediaTypeHeader(string? mediaType, Encoding encoding)
	{
		if (encoding == null)
			throw new ArgumentNullException(nameof(encoding));

		MediaType = mediaType;
		CharSet = encoding.WebName;
	}

	public MediaTypeHeaderValue ToMediaTypeHeaderValue()
	{
		var mediaTypeHeaderValue =
			new MediaTypeHeaderValue(MediaType!)
			{
				CharSet = string.IsNullOrWhiteSpace(CharSet) ? DefaultStringEncoding.WebName : CharSet
			};

		if (Parameters != null && 0 < Parameters.Count)
			foreach (var parameter in Parameters)
				mediaTypeHeaderValue.Parameters.Add(parameter.ToNameValueHeaderValue());

		return mediaTypeHeaderValue;
	}
}
