using Envelope.NetHttp.Http.Headers;

namespace Envelope.NetHttp.Http;

public abstract class ContentBase
{
	public ContentHeaders Headers { get; }
	public bool ClearDefaultHeaders { get; set; }

	public ContentBase()
	{
		Headers = new ContentHeaders();
	}
}
