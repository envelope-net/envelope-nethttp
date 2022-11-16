using System.Text;

namespace Envelope.NetHttp.Http;

public class HttpContentDto
{
	public const string STRING_CONTENT_DELIMITER = "____________________________________________________________________________";
	public const string STRING_CONTENT = nameof(STRING_CONTENT);
	public const string JSON_CONTENT = nameof(JSON_CONTENT);
	public const string STREAM_CONTENT = nameof(STREAM_CONTENT);
	public const string BYTE_ARRAY_CONTENT = nameof(BYTE_ARRAY_CONTENT);
	public const string HTTP_CONTENT = nameof(HTTP_CONTENT);

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

	public async Task<string?> ContentsToStringAsync(string? contentDelimiter = STRING_CONTENT_DELIMITER)
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

			var count = 0;
			if (StringContents != null)
			{
				int idx = 0;
				foreach (var stringContent in StringContents)
				{
					count++;
					sb.AppendLine();
					sb.AppendLine($"[{count}] ... {STRING_CONTENT}[{idx}]:");
					sb.AppendLine(contentDelimiter);
					var content = await stringContent.ToStringAsync();
					sb.AppendLine(content ?? string.Empty);
					idx++;
				}
			}

			if (JsonContents != null)
			{
				int idx = 0;
				foreach (var jsonContent in JsonContents)
				{
					count++;
					sb.AppendLine();
					sb.AppendLine($"[{count}] ... {JSON_CONTENT}[{idx}]:");
					sb.AppendLine(contentDelimiter);
					var content = await jsonContent.ToStringAsync();
					sb.AppendLine(content ?? string.Empty);
					idx++;
				}
			}

			if (StreamContents != null)
			{
				int idx = 0;
				foreach (var streamContent in StreamContents)
				{
					count++;
					sb.AppendLine();
					sb.AppendLine($"[{count}] ... {STREAM_CONTENT}[{idx}]:");
					sb.AppendLine(contentDelimiter);
					var content = await streamContent.ToStringAsync();
					sb.AppendLine(content ?? string.Empty);
					idx++;
				}
			}

			if (ByteArrayContents != null)
			{
				int idx = 0;
				foreach (var byteArrayContent in ByteArrayContents)
				{
					count++;
					sb.AppendLine();
					sb.AppendLine($"[{count}] ... {BYTE_ARRAY_CONTENT}[{idx}]:");
					sb.AppendLine(contentDelimiter);
					var content = await byteArrayContent.ToStringAsync();
					sb.AppendLine(content ?? string.Empty);
					idx++;
				}
			}

			if (HttpContents != null)
			{
				int idx = 0;
				foreach (var httpContent in HttpContents)
				{
					count++;
					sb.AppendLine();
					sb.AppendLine($"[{count}] ... {HTTP_CONTENT}[{idx}]:");
					sb.AppendLine(contentDelimiter);
					var content = await httpContent.ToStringAsync();
					sb.AppendLine(content ?? string.Empty);
					idx++;
				}
			}

			return sb.ToString();
		}
		else
		{
			var stringContent = StringContents?.FirstOrDefault();
			if (stringContent != null)
				return await stringContent.ToStringAsync();

			var jsonContent = JsonContents?.FirstOrDefault();
			if (jsonContent?.Content != null)
				return await jsonContent.ToStringAsync();

			var streamContent = StreamContents?.FirstOrDefault();
			if (streamContent?.Stream != null)
				return await streamContent.ToStringAsync();

			var byteArrayContent = ByteArrayContents?.FirstOrDefault();
			if (byteArrayContent?.ByteArray != null)
				return await byteArrayContent.ToStringAsync();

			var httpContent = HttpContents?.FirstOrDefault();
			if (httpContent?.Stream != null)
				return await httpContent.ToStringAsync();
		}

		return null;
	}
}
