using Envelope.Extensions;
using System.Text;

namespace Envelope.NetHttp.Http;

public class HttpContent : ContentBase
{
	private System.Net.Http.HttpContent? _httpContent;

	public Stream? Stream { get; set; }

	public static HttpContent FromHttpContent(System.Net.Http.HttpContent httpContent)
	{
		if (httpContent == null)
			throw new ArgumentNullException(nameof(httpContent));

		var result = new HttpContent
		{
			_httpContent = httpContent
		};

		return result;
	}

	private bool contentHasBeenRead = false;
	internal async Task<HttpContent> ReadContentAsync()
	{
		if (contentHasBeenRead || _httpContent == null)
			return this;

		var bytes = await _httpContent.ReadAsByteArrayAsync().ConfigureAwait(false);
		Stream = new MemoryStream(bytes);
		Stream.Seek(0, SeekOrigin.Begin);

		contentHasBeenRead = true;
		return this;
	}

	public override Task<string?> ToStringAsync()
		=> Stream != null
			? Stream.ToStringAsync(Encoding.UTF8, true)
			: Task.FromResult((string?)null);
}
