#if NETSTANDARD2_0 || NETSTANDARD2_1
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

	[Newtonsoft.Json.JsonIgnore]
	public Exception? Exception { get; set; }

	public string? ExceptionText => Exception?.ToStringTrace();

	[Newtonsoft.Json.JsonIgnore]
	public bool StatusCodeIsOK =>
	StatusCode.HasValue
		&& StatusCode.Value < 400;

	[Newtonsoft.Json.JsonIgnore]
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
			return HttpResponseMessage.Content.CopyToAsync(stream);

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
			: HttpResponseMessage.Content.ReadAsStreamAsync();

	public Task<byte[]?> ReadContentAsByteArrayAsync(CancellationToken cancellationToken)
		=> HttpResponseMessage?.Content == null
			? Task.FromResult((byte[]?)null)
			: HttpResponseMessage.Content.ReadAsByteArrayAsync();

	public Task<string?> ReadContentAsStringAsync(CancellationToken cancellationToken)
		=> HttpResponseMessage?.Content == null
			? Task.FromResult((string?)null)
			: HttpResponseMessage.Content.ReadAsStringAsync();

	public async Task<T?> ReadJsonContentAsAsync<T>(
		Newtonsoft.Json.JsonSerializerSettings? jsonSerializerOptions = null,
		CancellationToken cancellationToken = default)
	{
		if (HttpResponseMessage == null)
			return default;

		//var ms = new MemoryStream();
		//await Response.Content.CopyToAsync(ms).ConfigureAwait(false);
		//ms.Seek(0, SeekOrigin.Begin);

		using var stream = await HttpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
		using var streamReader = new StreamReader(stream, new System.Text.UTF8Encoding(false));
		using var jsonTextReader = new Newtonsoft.Json.JsonTextReader(streamReader);
		var serializer = Newtonsoft.Json.JsonSerializer.Create(jsonSerializerOptions);

		var result = serializer.Deserialize<T>(jsonTextReader);
		return result;
	}

	public override string ToString()
	{
		return Newtonsoft.Json.JsonConvert.SerializeObject(this, new Newtonsoft.Json.JsonSerializerSettings { Formatting = Newtonsoft.Json.Formatting.Indented });
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
