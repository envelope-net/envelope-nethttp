namespace Envelope.NetHttp.Http;

public class ByteArrayContent : ContentBase
{
	public byte[]? ByteArray { get; set; }
	public string? HttpContentNameFormDataMultipartPurposeMultipartPurpose { get; set; }
	public string? HttpContentFileNameFormDataMultipartPurposeMultipartPurpose { get; set; }

	public System.Net.Http.ByteArrayContent ToByteArrayContent()
	{
		if (ByteArray == null)
			throw new InvalidOperationException($"{nameof(ByteArray)} == null");

		var content = new System.Net.Http.ByteArrayContent(ByteArray);

		if (ClearDefaultHeaders)
			content.Headers.Clear();
		
		Headers.SetHttpContentHeaders(content.Headers);

		return content;
	}
}
