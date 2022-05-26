using System.Net.Http.Headers;

namespace Envelope.NetHttp.Http.Headers;

public class RangeHeader
{
	public long? From { get; set; }
	public long? To { get; set; }
	public List<RangeItemHeader>? Ranges { get; }
	public string? Unit { get; set; }

	public RangeHeaderValue ToRangeHeaderValue()
	{
		var rangeHeaderValue = (From.HasValue || To.HasValue)
			? new RangeHeaderValue(From, To)
			: new RangeHeaderValue();

		if (!string.IsNullOrWhiteSpace(Unit))
			rangeHeaderValue.Unit = Unit;

		if (Ranges != null && 0 < Ranges.Count)
			foreach (var range in Ranges)
				rangeHeaderValue.Ranges.Add(range.ToRangeItemHeaderValue());

		return rangeHeaderValue;
	}
}
