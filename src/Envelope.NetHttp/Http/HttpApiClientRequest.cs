using Envelope.NetHttp.Http.Headers;
using Envelope.Web;

namespace Envelope.NetHttp.Http;

public class HttpApiClientRequest : IHttpApiClientRequest
{
	public const string MultipartFormData = "form-data";

	public string? BaseAddress { get; set; }
	public string? RelativePath { get; set; }
	public string? QueryString { get; set; }
	public string? HttpMethod { get; set; }
	public bool ClearDefaultHeaders { get; set; }
	public RequestHeaders Headers { get; }
	public string? MultipartSubType { get; set; }
	public string? MultipartBoundary { get; set; }
	public TimeSpan? RequestTimeout { get; set; }
	public List<KeyValuePair<string, string>>? FormData { get; set; }
	public List<StringContent>? StringContents { get; set; }
	public List<JsonContent>? JsonContents { get; set; }
	public List<StreamContent>? StreamContents { get; set; }
	public List<ByteArrayContent>? ByteArrayContents { get; set; }

	public HttpApiClientRequest()
	{
		Headers = new RequestHeaders();
	}

	public HttpRequestMessage ToHttpRequestMessage()
	{
		if (HttpMethod == null)
			throw new InvalidOperationException($"{HttpMethod} == null");

		var path = GetRequestUri();
		var httpRequestMessage = new HttpRequestMessage(new System.Net.Http.HttpMethod(HttpMethod), path);

		if (ClearDefaultHeaders)
			httpRequestMessage.Headers.Clear();

		Headers.SetHttpRequestHeaders(httpRequestMessage.Headers);

		var content = ToHttpContent();
		if (content != null)
			httpRequestMessage.Content = content;

		return httpRequestMessage;
	}

	public string? GetRequestUri()
		=> UriHelper.Combine(
			string.IsNullOrWhiteSpace(BaseAddress) ? "/" : BaseAddress,
			$"{RelativePath}{QueryString}");

	public HttpContent? ToHttpContent()
	{
		var contentsCount =
			(FormData?.Count ?? 0)
			+ (StringContents?.Count ?? 0)
			+ (JsonContents?.Count ?? 0)
			+ (StreamContents?.Count ?? 0)
			+ (ByteArrayContents?.Count ?? 0);

		if (contentsCount == 0)
			return null;

		if (1 < contentsCount)
		{
			if (string.IsNullOrWhiteSpace(MultipartSubType))
				throw new InvalidOperationException($"{nameof(MultipartSubType)} == null");

			MultipartContent multipartContent;
			if (string.Equals(MultipartFormData, MultipartSubType, StringComparison.OrdinalIgnoreCase))
			{
				var multipartFormDataContent = string.IsNullOrWhiteSpace(MultipartBoundary)
					? new MultipartFormDataContent()
					: new MultipartFormDataContent(MultipartBoundary);

				if (FormData != null)
					foreach (var kvp in FormData)
						multipartFormDataContent.Add(new System.Net.Http.StringContent(kvp.Value), kvp.Key);

				if (StringContents != null)
					foreach (var stringContent in StringContents)
					{
						if (string.IsNullOrWhiteSpace(stringContent.HttpContentNameFormDataMultipartPurpose))
							multipartFormDataContent.Add(stringContent.ToStringContent());
						else
							multipartFormDataContent.Add(stringContent.ToStringContent(), stringContent.HttpContentNameFormDataMultipartPurpose);
					}

				if (JsonContents != null)
					foreach (var jsonContent in JsonContents)
					{
						if (string.IsNullOrWhiteSpace(jsonContent.HttpContentNameFormDataMultipartPurpose))
							multipartFormDataContent.Add(jsonContent.ToJsonContent());
						else
							multipartFormDataContent.Add(jsonContent.ToJsonContent(), jsonContent.HttpContentNameFormDataMultipartPurpose);
					}

				if (StreamContents != null)
					foreach (var streamContent in StreamContents)
					{
						if (string.IsNullOrWhiteSpace(streamContent.HttpContentNameFormDataMultipartPurposeMultipartPurpose))
							multipartFormDataContent.Add(streamContent.ToStreamContent());
						else if (string.IsNullOrWhiteSpace(streamContent.HttpContentFileNameFormDataMultipartPurposeMultipartPurpose))
							multipartFormDataContent.Add(streamContent.ToStreamContent(), streamContent.HttpContentNameFormDataMultipartPurposeMultipartPurpose);
						else
							multipartFormDataContent.Add(streamContent.ToStreamContent(), streamContent.HttpContentNameFormDataMultipartPurposeMultipartPurpose, streamContent.HttpContentFileNameFormDataMultipartPurposeMultipartPurpose);
					}

				if (ByteArrayContents != null)
					foreach (var byteArrayContent in ByteArrayContents)
					{
						if (string.IsNullOrWhiteSpace(byteArrayContent.HttpContentNameFormDataMultipartPurposeMultipartPurpose))
							multipartFormDataContent.Add(byteArrayContent.ToByteArrayContent());
						else if (string.IsNullOrWhiteSpace(byteArrayContent.HttpContentFileNameFormDataMultipartPurposeMultipartPurpose))
							multipartFormDataContent.Add(byteArrayContent.ToByteArrayContent(), byteArrayContent.HttpContentNameFormDataMultipartPurposeMultipartPurpose);
						else
							multipartFormDataContent.Add(byteArrayContent.ToByteArrayContent(), byteArrayContent.HttpContentNameFormDataMultipartPurposeMultipartPurpose, byteArrayContent.HttpContentFileNameFormDataMultipartPurposeMultipartPurpose);
					}

				multipartContent = multipartFormDataContent;
			}
			else
			{
				multipartContent = string.IsNullOrWhiteSpace(MultipartBoundary)
					? new MultipartContent(MultipartSubType)
					: new MultipartContent(MultipartSubType, MultipartBoundary);

				if (FormData != null)
					foreach (var kvp in FormData)
						throw new InvalidOperationException($"{nameof(FormData)} must be send as {nameof(MultipartFormDataContent)}. {nameof(MultipartSubType)} must be equal to {MultipartFormData}");

				if (StringContents != null)
					foreach (var stringContent in StringContents)
						multipartContent.Add(stringContent.ToStringContent());

				if (JsonContents != null)
					foreach (var jsonContent in JsonContents)
						multipartContent.Add(jsonContent.ToJsonContent());

				if (StreamContents != null)
					foreach (var streamContent in StreamContents)
						multipartContent.Add(streamContent.ToStreamContent());

				if (ByteArrayContents != null)
					foreach (var byteArrayContent in ByteArrayContents)
						multipartContent.Add(byteArrayContent.ToByteArrayContent());
			}

			return multipartContent;
		}
		else //if (contentsCount == 1)
		{
			if (FormData != null && 0 < FormData.Count)
				throw new InvalidOperationException($"{nameof(FormData)} must be send as {nameof(MultipartFormDataContent)}");

			var stringContent = StringContents?.FirstOrDefault();
			if (stringContent != null)
				return stringContent.ToStringContent();

			var jsonContent = JsonContents?.FirstOrDefault();
			if (jsonContent != null)
				return jsonContent.ToJsonContent();

			var streamContent = StreamContents?.FirstOrDefault();
			if (streamContent != null)
				return streamContent.ToStreamContent();

			var byteArrayContent = ByteArrayContents?.FirstOrDefault();
			if (byteArrayContent != null)
				return byteArrayContent.ToByteArrayContent();
		}

		return null;
	}
}
