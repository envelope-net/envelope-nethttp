using System.Net.Http.Headers;

namespace Envelope.NetHttp.Http.Headers;

public class RangeConditionHeader
{
	public DateTimeOffset? Date { get; set; }
	public EntityTagHeader? EntityTag { get; set; }

	public RangeConditionHeaderValue ToRangeConditionHeaderValue()
	{
		if (EntityTag != null)
			return new RangeConditionHeaderValue(EntityTag.ToEntityTagHeaderValue());
		else if (Date.HasValue)
			return new RangeConditionHeaderValue(Date.Value);
		else
			throw new InvalidOperationException($"{nameof(EntityTag)} == null && {nameof(Date)} == null");
	}
}
