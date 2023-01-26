using Envelope.Extensions;
using Envelope.Web.Logging;

namespace Envelope.NetHttp.Http;

public static class RequestDtoMapper
{
	public static async Task<RequestDto> MapAsync(
		HttpRequestMessage httpRequest,
		string? remoteIp,
		Guid? correlationId,
		string? externalCorrelationId,
		bool logRequestHeaders,
		bool logRequestBodyAsString,
		bool logRequestBodyAsByteArray,
		CancellationToken cancellationToken)
	{
		if (httpRequest == null)
			throw new ArgumentNullException(nameof(httpRequest));

		var request = new RequestDto
		{
			CorrelationId = correlationId,
			ExternalCorrelationId = externalCorrelationId,
			RemoteIp = remoteIp
		};

		try { request.Method = httpRequest.Method.Method; } catch { }
		try { request.Path = httpRequest.RequestUri?.ToString(); } catch { }
		try { request.ContentType = httpRequest.Content?.Headers?.ContentType?.ToString(); } catch { }

		if (logRequestHeaders)
		{
			try
			{
				if (httpRequest.Headers != null)
				{
					var headers = httpRequest.Headers.ToDictionary(x => x.Key, x => x.Value);

					if (httpRequest.Content?.Headers != null)
					{
						var contentHeaders = httpRequest.Content.Headers.ToDictionary(x => x.Key, x => x.Value);
						headers.AddOrReplaceRange(contentHeaders);
					}

#if NETSTANDARD2_0 || NETSTANDARD2_1
					request.Headers = Newtonsoft.Json.JsonConvert.SerializeObject(headers);
#elif NET6_0_OR_GREATER
					request.Headers = System.Text.Json.JsonSerializer.Serialize(headers);
#endif
				}
			}
			catch { }
		}

		if (logRequestBodyAsString)
		{
			if (httpRequest.Content != null)
				request.Body = await httpRequest.Content.ReadAsStringAsync(
#if NET6_0_OR_GREATER
					cancellationToken
#endif
					).ConfigureAwait(false);

			if (string.IsNullOrWhiteSpace(request.Body))
				request.Body = null;
		}

		if (logRequestBodyAsByteArray)
		{
			if (httpRequest.Content != null)
				request.BodyByteArray = await httpRequest.Content.ReadAsByteArrayAsync(
#if NET6_0_OR_GREATER
					cancellationToken
#endif
					).ConfigureAwait(false);

			if (request.BodyByteArray != null && request.BodyByteArray.Length == 0)
				request.BodyByteArray = null;
		}

		return request;
	}
}
