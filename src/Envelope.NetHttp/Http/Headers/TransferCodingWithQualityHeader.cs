using System.Net.Http.Headers;

namespace Envelope.NetHttp.Http.Headers;

public class TransferCodingWithQualityHeader : TransferCodingHeader
{
	public double? Quality { get; set; }

	public TransferCodingWithQualityHeaderValue ToTransferCodingWithQualityHeaderValue()
	{
		if (string.IsNullOrWhiteSpace(Value))
			throw new InvalidOperationException($"{nameof(Value)} == null");

		var transferCodingWithQualityHeaderValue = 
			Quality.HasValue
				? new TransferCodingWithQualityHeaderValue(Value, Quality.Value)
				: new TransferCodingWithQualityHeaderValue(Value);

		if (Parameters != null && 0 < Parameters.Count)
			foreach (var parameter in Parameters)
				transferCodingWithQualityHeaderValue.Parameters.Add(parameter.ToNameValueHeaderValue());

		return transferCodingWithQualityHeaderValue;
	}
}
