using System.Net.Http.Headers;

namespace Envelope.NetHttp.Http.Headers;

public class TransferCodingHeader
{
	public string? Value { get; set; }
	public List<NameValueHeader>? Parameters { get; set; }

	public TransferCodingHeaderValue ToTransferCodingHeaderValue()
	{
		if (string.IsNullOrWhiteSpace(Value))
			throw new InvalidOperationException($"{nameof(Value)} == null");

		var transferCodingHeaderValue = new TransferCodingHeaderValue(Value);

		if (Parameters != null && 0 < Parameters.Count)
			foreach (var parameter in Parameters)
				transferCodingHeaderValue.Parameters.Add(parameter.ToNameValueHeaderValue());

		return transferCodingHeaderValue;
	}
}
