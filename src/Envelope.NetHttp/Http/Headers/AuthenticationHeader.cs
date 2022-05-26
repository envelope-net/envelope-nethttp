using System.Net.Http.Headers;

namespace Envelope.NetHttp.Http.Headers;

public class AuthenticationHeader
{
	public string? Scheme { get; set; }
	public string? Parameter { get; set; }

	public AuthenticationHeaderValue ToAuthenticationHeaderValue()
	{
		if (string.IsNullOrWhiteSpace(Scheme))
			throw new InvalidOperationException($"{nameof(Scheme)} == null");

		return string.IsNullOrWhiteSpace(Parameter)
			? new AuthenticationHeaderValue(Scheme)
			: new AuthenticationHeaderValue(Scheme, Parameter);
	}
}
