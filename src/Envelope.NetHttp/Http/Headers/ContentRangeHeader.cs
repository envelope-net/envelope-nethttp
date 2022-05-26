using System.Net.Http.Headers;

namespace Envelope.NetHttp.Http.Headers;

public class ContentRangeHeader
{
	public long? From { get; set; }
	public long? To { get; set; }
	public long? Length { get; set; }
	public string? Unit { get; set; }

	public ContentRangeHeaderValue ToContentRangeHeaderValue()
	{
		ContentRangeHeaderValue? contentRangeHeaderValue;
		if (From.HasValue)
		{
			if (To.HasValue)
			{
				if (Length.HasValue)
				{
					contentRangeHeaderValue = new ContentRangeHeaderValue(From.Value, To.Value, Length.Value);
				}
				else
				{
					contentRangeHeaderValue = new ContentRangeHeaderValue(From.Value, To.Value);
				}
			}
			else
			{
				throw new InvalidOperationException($"{nameof(From)} == {From} && {nameof(To)} == null");
			}
		}
		else
		{
			if (Length.HasValue)
			{
				contentRangeHeaderValue = new ContentRangeHeaderValue(Length.Value);
			}
			else
			{
				throw new InvalidOperationException($"{nameof(From)} == null && {nameof(Length)} == null");
			}
		}

		if (!string.IsNullOrWhiteSpace(Unit))
			contentRangeHeaderValue.Unit = Unit;

		return contentRangeHeaderValue;
	}
}
