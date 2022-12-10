using Envelope.Extensions;
using System.Text;

namespace Envelope.NetHttp.Http;

public class StreamContent : ContentBase
{
	private System.Net.Http.StreamContent? _streamContent;

	public Stream? Stream { get; set; }
	public string? HttpContentNameFormDataMultipartPurposeMultipartPurpose { get; set; }
	public string? HttpContentFileNameFormDataMultipartPurposeMultipartPurpose { get; set; }

	public static StreamContent FromStreamContent(System.Net.Http.StreamContent streamContent)
	{
		if (streamContent == null)
			throw new ArgumentNullException(nameof(streamContent));

		var result = new StreamContent
		{
			_streamContent = streamContent
		};

		return result;
	}

	private bool contentHasBeenRead = false;
	internal async Task<StreamContent> ReadContentAsync()
	{
		if (contentHasBeenRead || _streamContent == null)
			return this;

		var bytes = await _streamContent.ReadAsByteArrayAsync().ConfigureAwait(false);
		Stream = new MemoryStream(bytes);
		Stream.Seek(0, SeekOrigin.Begin);

		//Stream = new MemoryStream();
		//await _streamContent.CopyToAsync(Stream).ConfigureAwait(false);
		//Stream.Seek(0, SeekOrigin.Begin);

		contentHasBeenRead = true;
		return this;
	}

	public System.Net.Http.StreamContent ToStreamContent()
	{
		if (_streamContent != null)
			return _streamContent;

		if (Stream == null)
			throw new InvalidOperationException($"{nameof(Stream)} == null");

		var content = new System.Net.Http.StreamContent(Stream);

		if (ClearDefaultHeaders)
			content.Headers.Clear();

		Headers.SetHttpContentHeaders(content.Headers);

		return content;
	}

	public override Task<string?> ToStringAsync()
		=> Stream != null
			? Stream.ToStringAsync(Encoding.UTF8, true)
			: Task.FromResult((string?)null);
}
