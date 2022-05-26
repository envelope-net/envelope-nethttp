using System.Net.Http.Headers;

namespace Envelope.NetHttp.Http.Headers;

public class WarningHeader
{
	public int? Code { get; set; }
	public string? Agent { get; set; }
	public string? Text { get; set; }
	public DateTimeOffset? Date { get; set; }

	public WarningHeaderValue ToWarningHeaderValue()
	{
		if (!Code.HasValue)
			throw new InvalidOperationException($"{nameof(Code)} == null");

		if (string.IsNullOrWhiteSpace(Agent))
			throw new InvalidOperationException($"{nameof(Agent)} == null");

		if (string.IsNullOrWhiteSpace(Text))
			throw new InvalidOperationException($"{nameof(Text)} == null");

		return Date.HasValue
			? new WarningHeaderValue(Code.Value, Agent, Text, Date.Value)
			: new WarningHeaderValue(Code.Value, Agent, Text);
	}
}
