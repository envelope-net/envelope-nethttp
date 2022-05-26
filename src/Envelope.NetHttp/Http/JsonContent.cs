using System.Net.Http.Headers;

namespace Envelope.NetHttp.Http;

public class JsonContent : ContentBase
{
	public object? Content { get; set; }
	public Type? InputType { get; set; }
	public MediaTypeHeaderValue? MediaType { get; set; }
#if NET6_0_OR_GREATER
	public System.Text.Json.JsonSerializerOptions? JsonSerializerOptions { get; set; }
#elif NETSTANDARD2_0 || NETSTANDARD2_1
	public Newtonsoft.Json.JsonSerializerSettings? JsonSerializerOptions { get; set; }
#endif
	public string? HttpContentNameFormDataMultipartPurpose { get; set; }

	public System.Net.Http.Json.JsonContent ToJsonContent()
	{
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
}
