using System.Text;

namespace Envelope.NetHttp.Http;

public class StringContent : ContentBase
{
	private System.Net.Http.StringContent? _stringContent;

	public string? Content { get; set; }
	public Encoding? Encoding { get; set; }
	public string? MediaType { get; set; }
	public string? HttpContentNameFormDataMultipartPurpose { get; set; }

	public static StringContent FromStringContent(System.Net.Http.StringContent stringContent)
	{
		if (stringContent == null)
			throw new ArgumentNullException(nameof(stringContent));

		var result = new StringContent
		{
			_stringContent = stringContent,
			Encoding = null,
			MediaType = stringContent.Headers?.ContentType?.ToString()
		};
		
		return result;
	}

	private bool contentHasBeenRead = false;
	internal async Task<StringContent> ReadContentAsync()
	{
		if (contentHasBeenRead || _stringContent == null)
			return this;

		Content = await _stringContent.ReadAsStringAsync();
		contentHasBeenRead = true;
		return this;
	}

	public System.Net.Http.StringContent ToStringContent()
	{
		if (_stringContent != null)
			return _stringContent;

		if (string.IsNullOrWhiteSpace(Content))
			throw new InvalidOperationException($"{nameof(Content)} == null");

		var content =  new System.Net.Http.StringContent(Content, Encoding, MediaType);

		if (ClearDefaultHeaders)
			content.Headers.Clear();

		Headers.SetHttpContentHeaders(content.Headers);

		return content;
	}
}
