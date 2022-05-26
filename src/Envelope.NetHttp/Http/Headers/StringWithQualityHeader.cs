using System.Net.Http.Headers;

namespace Envelope.NetHttp.Http.Headers;

public class StringWithQualityHeader
{
	public string? Value { get; set; }
	public double? Quality { get; set; }

	public StringWithQualityHeaderValue ToStringWithQualityHeaderValue()
	{
		if (string.IsNullOrWhiteSpace(Value))
			throw new InvalidOperationException($"{nameof(Value)} == null");

		return Quality.HasValue
			? new StringWithQualityHeaderValue(Value, Quality.Value)
			: new StringWithQualityHeaderValue(Value);
	}
}
