using System.Net.Http.Headers;

namespace Envelope.NetHttp.Http;

public class JsonContent : ContentBase
{
	internal System.Net.Http.Json.JsonContent? _jsonContent;

	public object? Content { get; set; }
	public Type? InputType { get; set; }
	public MediaTypeHeaderValue? MediaType { get; set; }
#if NET6_0_OR_GREATER
	public System.Text.Json.JsonSerializerOptions? JsonSerializerOptions { get; set; }
#elif NETSTANDARD2_0 || NETSTANDARD2_1
	public Newtonsoft.Json.JsonSerializerSettings? JsonSerializerOptions { get; set; }
#endif
	public string? HttpContentNameFormDataMultipartPurpose { get; set; }

	public static JsonContent FromJsonContent(System.Net.Http.Json.JsonContent jsonContent)
	{
		if (jsonContent == null)
			throw new ArgumentNullException(nameof(jsonContent));

		var result = new JsonContent
		{
			_jsonContent = jsonContent,
			MediaType = jsonContent.Headers?.ContentType
		};

		return result;
	}

	private bool contentHasBeenRead = false;
	internal async Task<JsonContent> ReadContentAsync()
	{
		if (contentHasBeenRead || _jsonContent == null)
			return this;

		if (InputType != null)
		{
#if NET6_0_OR_GREATER
			using var stream = await _jsonContent.ReadAsStreamAsync().ConfigureAwait(false);
			Content = await System.Text.Json.JsonSerializer.DeserializeAsync(stream, InputType, JsonSerializerOptions).ConfigureAwait(false);
#elif NETSTANDARD2_0 || NETSTANDARD2_1
				var json = await _jsonContent.ReadAsStringAsync().ConfigureAwait(false);
				Content = Newtonsoft.Json.JsonConvert.DeserializeObject(json, InputType, JsonSerializerOptions);
#endif
		}
		else
		{
			Content = await _jsonContent.ReadAsStringAsync();
		}

		contentHasBeenRead = true;
		return this;
	}

	public System.Net.Http.Json.JsonContent ToJsonContent()
	{
		if (_jsonContent != null)
			return _jsonContent;

		if (InputType == null)
			throw new InvalidOperationException($"{nameof(InputType)} == null");

		var content = System.Net.Http.Json.JsonContent.Create(Content, InputType, MediaType, JsonSerializerOptions);

		if (ClearDefaultHeaders)
			content.Headers.Clear();

		Headers.SetHttpContentHeaders(content.Headers);

		return content;
	}
}

public class JsonContent<T> : JsonContent
{
	public JsonContent()
	{
		InputType = typeof(T);
	}

	public static async Task<JsonContent<T>> SetJsonContentAsync(System.Net.Http.Json.JsonContent jsonContent, bool readPayload)
	{
		var result = new JsonContent<T>
		{
			_jsonContent = jsonContent ?? throw new ArgumentNullException(nameof(jsonContent)),
			MediaType = jsonContent.Headers?.ContentType
		};

		if (readPayload)
		{
#if NET6_0_OR_GREATER
			using var stream = await result._jsonContent.ReadAsStreamAsync().ConfigureAwait(false);
			result.Content = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(stream, result.JsonSerializerOptions).ConfigureAwait(false);
#elif NETSTANDARD2_0 || NETSTANDARD2_1
			var json = await result._jsonContent.ReadAsStringAsync().ConfigureAwait(false);
			result.Content = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, result.JsonSerializerOptions);
#endif
		}

		return result;
	}
}
