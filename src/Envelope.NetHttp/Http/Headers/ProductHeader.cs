using System.Net.Http.Headers;

namespace Envelope.NetHttp.Http.Headers;

public class ProductHeader
{
	public string? Name { get; set; }
	public string? Version { get; set; }

	public ProductHeaderValue ToProductHeaderValue()
	{
		if (string.IsNullOrWhiteSpace(Name))
			throw new InvalidOperationException($"{nameof(Name)} == null");

		return string.IsNullOrWhiteSpace(Version)
			? new ProductHeaderValue(Name)
			: new ProductHeaderValue(Name, Version);
	}
}
