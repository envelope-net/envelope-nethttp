namespace Envelope.NetHttp.Http;

public class StreamContent : ContentBase
{
	public Stream? Stream { get; set; }
	public string? HttpContentNameFormDataMultipartPurposeMultipartPurpose { get; set; }
	public string? HttpContentFileNameFormDataMultipartPurposeMultipartPurpose { get; set; }

	public System.Net.Http.StreamContent ToStreamContent()
	{
		if (Stream == null)
			throw new InvalidOperationException($"{nameof(Stream)} == null");

		var content = new System.Net.Http.StreamContent(Stream);

		if (ClearDefaultHeaders)
			content.Headers.Clear();

		Headers.SetHttpContentHeaders(content.Headers);

		return content;
	}
}
