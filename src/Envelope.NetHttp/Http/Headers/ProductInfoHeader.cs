using System.Net.Http.Headers;

namespace Envelope.NetHttp.Http.Headers;

public class ProductInfoHeader
{
	public string? Comment { get; set; }
	public ProductHeader? Product { get; set; }

	public ProductInfoHeaderValue ToProductInfoHeaderValue()
	{
		if (Product != null)
			return new ProductInfoHeaderValue(Product.ToProductHeaderValue());
		else if (!string.IsNullOrWhiteSpace(Comment))
			return new ProductInfoHeaderValue(Comment);
		else
			throw new InvalidOperationException($"{nameof(Product)} == null && {nameof(Comment)} == null");
	}
}
