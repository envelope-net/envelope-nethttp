using System.Net.Http.Headers;

namespace Envelope.NetHttp.Http.Headers;

public class ViaHeader
{
	public string? ProtocolVersion { get; set; }
	public string? ReceivedBy { get; set; }
	public string? ProtocolName { get; set; }
	public string? Comment { get; set; }

	public ViaHeaderValue ToViaHeaderValue()
	{
		if (string.IsNullOrWhiteSpace(ProtocolVersion))
			throw new InvalidOperationException($"{nameof(ProtocolVersion)} == null");

		if (string.IsNullOrWhiteSpace(ReceivedBy))
			throw new InvalidOperationException($"{nameof(ReceivedBy)} == null");

		if (string.IsNullOrWhiteSpace(ProtocolName))
		{
			if (string.IsNullOrWhiteSpace(Comment))
				return new ViaHeaderValue(ProtocolVersion, ReceivedBy);
			else
				return new ViaHeaderValue(ProtocolVersion, ReceivedBy, ProtocolName, Comment);
		}
		else
		{
			if (string.IsNullOrWhiteSpace(Comment))
				return new ViaHeaderValue(ProtocolVersion, ReceivedBy, ProtocolName);
			else
				return new ViaHeaderValue(ProtocolVersion, ReceivedBy, ProtocolName, Comment);
		}
	}
}
