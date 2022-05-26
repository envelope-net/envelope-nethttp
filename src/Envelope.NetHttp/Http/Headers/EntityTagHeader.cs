using System.Net.Http.Headers;

namespace Envelope.NetHttp.Http.Headers;

public class EntityTagHeader
{
	public string? Tag { get; set; }
	public bool? IsWeak { get; set; }

	public EntityTagHeaderValue ToEntityTagHeaderValue()
	{
		if (string.IsNullOrWhiteSpace(Tag))
			throw new InvalidOperationException($"{nameof(Tag)} == null");

		return IsWeak.HasValue
			? new EntityTagHeaderValue(Tag, IsWeak.Value)
			: new EntityTagHeaderValue(Tag);
	}
}
