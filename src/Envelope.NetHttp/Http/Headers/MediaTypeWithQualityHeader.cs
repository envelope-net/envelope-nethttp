using System.Net.Http.Headers;
using System.Text;

namespace Envelope.NetHttp.Http.Headers;

public class MediaTypeWithQualityHeader : MediaTypeHeader
{
	public double? Quality { get; set; }

	public MediaTypeWithQualityHeader()
		: base()
	{
	}

	public MediaTypeWithQualityHeader(string? mediaType, Encoding encoding, double? quality)
		: base(mediaType, encoding)
	{
		Quality = quality;
	}

	public MediaTypeWithQualityHeaderValue ToMediaTypeWithQualityHeaderValue()
	{
		var mediaTypeWithQualityHeaderValue =
			Quality.HasValue
				? new MediaTypeWithQualityHeaderValue(MediaType!, Quality.Value)
				{
					CharSet = string.IsNullOrWhiteSpace(CharSet) ? DefaultStringEncoding.WebName : CharSet
				}
				: new MediaTypeWithQualityHeaderValue(MediaType!)
				{
					CharSet = string.IsNullOrWhiteSpace(CharSet) ? DefaultStringEncoding.WebName : CharSet
				};

		if (Parameters != null && 0 < Parameters.Count)
			foreach (var parameter in Parameters)
				mediaTypeWithQualityHeaderValue.Parameters.Add(parameter.ToNameValueHeaderValue());

		return mediaTypeWithQualityHeaderValue;
	}
}
