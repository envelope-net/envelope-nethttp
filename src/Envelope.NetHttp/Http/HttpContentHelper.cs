namespace Envelope.NetHttp.Http;

public static class HttpContentHelper
{
	public static HttpContentDto ParseHttpContent(System.Net.Http.HttpContent? httpContent)
	{
		var result = new HttpContentDto();

		if (httpContent == null)
			return result;

		if (httpContent is System.Net.Http.StringContent stringContent)
		{
			result.StringContents = new List<StringContent>
			{
				StringContent.FromStringContent(stringContent)
			};
		}
		else if (httpContent is System.Net.Http.Json.JsonContent jsonContent)
		{
			result.JsonContents = new List<JsonContent>
			{
				JsonContent.FromJsonContent(jsonContent)
			};
		}
		else if (httpContent is System.Net.Http.StreamContent streamContent)
		{
			result.StreamContents = new List<StreamContent>
			{
				StreamContent.FromStreamContent(streamContent)
			};
		}
		else if (httpContent is System.Net.Http.ByteArrayContent byteArrayContent)
		{
			result.ByteArrayContents = new List<ByteArrayContent>
			{
				ByteArrayContent.FromByteArrayContent(byteArrayContent)
			};
		}
		else if (httpContent is System.Net.Http.MultipartContent multipartContent)
		{
			//TODO dorobit
			throw new NotImplementedException($"Unknown content = {multipartContent.GetType().FullName}");
		}
		else
		{
			result.HttpContents = new List<HttpContent>
			{
				HttpContent.FromHttpContent(httpContent)
			};
		}

		return result;
	}
}
