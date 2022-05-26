#if NET6_0_OR_GREATER
using Envelope.Extensions;

namespace Envelope.NetHttp.Http.Internal;

internal class HttpApiClientResponse : IHttpApiClientResponse, IDisposable
{
	private bool disposedValue;

	public IHttpApiClientRequest Request { get; }
	public HttpResponseMessage? HttpResponseMessage { get; set; }

	public int? StatusCode => (int?)HttpResponseMessage?.StatusCode;

	public bool? RequestTimedOut { get; set; }

	public bool? OperationCanceled { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	public Exception? Exception { get; set; }

	public string? ExceptionText => Exception?.ToStringTrace();

	[System.Text.Json.Serialization.JsonIgnore]
	public bool StatusCodeIsOK =>
	StatusCode.HasValue
		&& StatusCode.Value < 400;

	[System.Text.Json.Serialization.JsonIgnore]
	public bool IsOK =>
		StatusCodeIsOK
		&& Exception == null
		&& OperationCanceled != true
		&& RequestTimedOut != true;

	public HttpApiClientResponse(IHttpApiClientRequest request)
	{
		Request = request ?? throw new ArgumentNullException(nameof(request));
	}

	public Task CopyContentToAsync(Stream stream, CancellationToken cancellationToken)
	{
		if (HttpResponseMessage?.Content != null)
			return HttpResponseMessage.Content.CopyToAsync(stream, cancellationToken);

		return Task.CompletedTask;
	}

	public List<KeyValuePair<string, IEnumerable<string>>>? GetAllHeaders()
	{
		var responseHeaders = GetResponseHeaders();

		if (responseHeaders == null)
		{
			return GetContentHeaders();
		}
		else
		{
			var contentHeader = GetContentHeaders();
			if (contentHeader != null)
				responseHeaders.AddRange(contentHeader);

			return responseHeaders;
		}
	}

	public List<KeyValuePair<string, IEnumerable<string>>>? GetResponseHeaders()
		=> HttpResponseMessage?.Headers?.ToList();

	public List<KeyValuePair<string, IEnumerable<string>>>? GetContentHeaders()
		=> HttpResponseMessage?.Content.Headers?.ToList();

	public Task<Stream?> ReadContentAsStreamAsync(CancellationToken cancellationToken)
		=> HttpResponseMessage?.Content == null
			? Task.FromResult((Stream?)null)
			: HttpResponseMessage.Content.ReadAsStreamAsync(cancellationToken) as Task<Stream?>;

	public Task<byte[]?> ReadContentAsByteArrayAsync(CancellationToken cancellationToken)
		=> HttpResponseMessage?.Content == null
			? Task.FromResult((byte[]?)null)
			: HttpResponseMessage.Content.ReadAsByteArrayAsync(cancellationToken) as Task<byte[]?>;

	public Task<string?> ReadContentAsStringAsync(CancellationToken cancellationToken)
		=> HttpResponseMessage?.Content == null
			? Task.FromResult((string?)null)
			: HttpResponseMessage.Content.ReadAsStringAsync(cancellationToken) as Task<string?>;

	public async Task<T?> ReadJsonContentAsAsync<T>(
		System.Text.Json.JsonSerializerOptions? jsonSerializerOptions = null, 
		CancellationToken cancellationToken = default)
	{
		if (HttpResponseMessage == null)
			return default;

		using var stream = await HttpResponseMessage.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
		var result = await System.Text.Json.JsonSerializer.DeserializeAsync<T>(stream, jsonSerializerOptions, cancellationToken).ConfigureAwait(false);
		return result;
	}

	public override string ToString()
	{
		return System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing && HttpResponseMessage != null)
			{
				HttpResponseMessage.Dispose();
			}

			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
#endif
