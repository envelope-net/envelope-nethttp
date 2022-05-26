using System.Text;

namespace Envelope.NetHttp.Http;

public class StringContent : ContentBase
{
	public string? Content { get; set; }
	public Encoding? Encoding { get; set; }
	public string? MediaType { get; set; }
	public string? HttpContentNameFormDataMultipartPurpose { get; set; }

	public System.Net.Http.StringContent ToStringContent()
	{
		if (string.IsNullOrWhiteSpace(Content))
			throw new InvalidOperationException($"{nameof(Content)} == null");

		var content =  new System.Net.Http.StringContent(Content, Encoding, MediaType);

		if (ClearDefaultHeaders)
			content.Headers.Clear();

		Headers.SetHttpContentHeaders(content.Headers);

		return content;
	}
}
