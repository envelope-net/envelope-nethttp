using System.Net.Http.Headers;

namespace Envelope.NetHttp.Http.Headers;

public class RangeItemHeader
{
	public long? From { get; set; }
	public long? To { get; set; }

	public RangeItemHeaderValue ToRangeItemHeaderValue()
		=> new(From, To);
}
