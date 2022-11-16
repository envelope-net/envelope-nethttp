using System.Net.Http;
using System.Text;

namespace Envelope.NetHttp.Http;

public class ByteArrayContent : ContentBase
{
	private System.Net.Http.ByteArrayContent? _byteArrayContent;

	public byte[]? ByteArray { get; set; }
	public string? HttpContentNameFormDataMultipartPurposeMultipartPurpose { get; set; }
	public string? HttpContentFileNameFormDataMultipartPurposeMultipartPurpose { get; set; }

	public static ByteArrayContent FromByteArrayContent(System.Net.Http.ByteArrayContent byteArrayContent)
	{
		if (byteArrayContent == null)
			throw new ArgumentNullException(nameof(byteArrayContent));

		var result = new ByteArrayContent
		{
			_byteArrayContent = byteArrayContent
		};

		return result;
	}

	private bool contentHasBeenRead = false;
	internal async Task<ByteArrayContent> ReadContentAsync()
	{
		if (contentHasBeenRead || _byteArrayContent == null)
			return this;

		ByteArray = await _byteArrayContent.ReadAsByteArrayAsync();
		contentHasBeenRead = true;
		return this;
	}

	public System.Net.Http.ByteArrayContent ToByteArrayContent()
	{
		if (_byteArrayContent != null)
			return _byteArrayContent;

		if (ByteArray == null)
			throw new InvalidOperationException($"{nameof(ByteArray)} == null");

		var content = new System.Net.Http.ByteArrayContent(ByteArray);

		if (ClearDefaultHeaders)
			content.Headers.Clear();
		
		Headers.SetHttpContentHeaders(content.Headers);

		return content;
	}

	public override Task<string?> ToStringAsync()
		=> ByteArray != null
			? Task.FromResult((string?)Encoding.UTF8.GetString(ByteArray))
			: Task.FromResult((string?)null);
}
