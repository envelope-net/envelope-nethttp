using Envelope.Extensions;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Envelope.NetHttp.Http;

public class HttpContentDto
{
	public List<StringContent>? StringContents { get; set; }
	public List<JsonContent>? JsonContents { get; set; }
	public List<StreamContent>? StreamContents { get; set; }
	public List<ByteArrayContent>? ByteArrayContents { get; set; }
	public List<HttpContent>? HttpContents { get; set; }

	internal async Task<HttpContentDto> ReadContentAsync()
	{
		if (0 < StringContents?.Count)
		{
			foreach (var stringContent in StringContents)
				await stringContent.ReadContentAsync();
		}

		if (0 < JsonContents?.Count)
		{
			foreach (var jsonContent in JsonContents)
				await jsonContent.ReadContentAsync();
		}

		if (0 < StreamContents?.Count)
		{
			foreach (var streamContent in StreamContents)
				await streamContent.ReadContentAsync();
		}

		if (0 < ByteArrayContents?.Count)
		{
			foreach (var byteArrayContent in ByteArrayContents)
				await byteArrayContent.ReadContentAsync();
		}

		if (0 < HttpContents?.Count)
		{
			foreach (var httpContents in HttpContents)
				await httpContents.ReadContentAsync();
		}

		return this;
	}

	public async Task<string?> ContentsToStringAsync()
	{
		_ = await ReadContentAsync();

		var contentsCount =
			(StringContents?.Count ?? 0)
			+ (JsonContents?.Count ?? 0)
			+ (StreamContents?.Count ?? 0)
			+ (ByteArrayContents?.Count ?? 0)
			+ (HttpContents?.Count ?? 0);

		if (contentsCount == 0)
			return null;

		if (1 < contentsCount)
		{
			var sb = new StringBuilder();

			return sb.ToString();
		}
		else
		{
			var stringContent = StringContents?.FirstOrDefault();
			if (stringContent != null)
				return stringContent.Content;

			var jsonContent = JsonContents?.FirstOrDefault();
			if (jsonContent?.Content != null)
				return jsonContent.Content.ToString();

			var streamContent = StreamContents?.FirstOrDefault();
			if (streamContent?.Stream != null)
				return await streamContent.Stream.ToStringAsync(Encoding.UTF8, true);

			var byteArrayContent = ByteArrayContents?.FirstOrDefault();
			if (byteArrayContent?.ByteArray != null)
				return Encoding.UTF8.GetString(byteArrayContent.ByteArray);

			var httpContent = HttpContents?.FirstOrDefault();
			if (httpContent?.Stream != null)
				return await httpContent.Stream.ToStringAsync(Encoding.UTF8, true);
		}

		return null;
	}
}
