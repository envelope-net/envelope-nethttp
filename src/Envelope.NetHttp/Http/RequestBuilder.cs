using Envelope.Extensions;
using Envelope.NetHttp.Http.Headers;

namespace Envelope.NetHttp.Http;

public interface IRequestBuilder<TBuilder, TObject>
	where TBuilder : IRequestBuilder<TBuilder, TObject>
	where TObject : IHttpApiClientRequest
{
	TBuilder Object(TObject request);

	TObject Build();

	HttpRequestMessage ToHttpRequestMessage();

	TBuilder BaseAddress(string? baseAddress, bool force = true);

	TBuilder RelativePath(string path, bool force = true);

	TBuilder QueryString(string? queryString, bool force = true);

	TBuilder QueryString(Dictionary<string, string>? queryString, bool force = true);

	TBuilder AddQueryString(string key, string value);

	TBuilder Method(string httpMethod, bool force = true);

	TBuilder Method(HttpMethod httpMethod, bool force = true);

	TBuilder ConfigureHeaders(Action<RequestHeaders> configure);

	TBuilder Multipart(string multipartSubType, string? multipartBoundary);

	TBuilder RequestTimeout(TimeSpan? requestTimeout, bool force = true);

	TBuilder AddFormData(KeyValuePair<string, string> formData, bool force = true);

	TBuilder AddFormData(List<KeyValuePair<string, string>> formData, bool force = true);

	TBuilder AddStringContent(StringContent stringContent, bool force = true);

	TBuilder AddStringContent(Func<StringContent, StringContent?> configureStringContent, bool force = true);

	TBuilder AddJsonContent(JsonContent jsonContent, bool force = true);

	TBuilder AddJsonContent(Func<JsonContent, JsonContent?> configureJsonContent, bool force = true);

	TBuilder AddJsonContent<T>(JsonContent<T> jsonContent, bool force = true);

	TBuilder AddJsonContent<T>(Func<JsonContent<T>, JsonContent<T>?> configureJsonContent, bool force = true);

	TBuilder AddStreamContent(StreamContent streamContent, bool force = true);

	TBuilder AddStreamContent(Func<StreamContent, StreamContent?> configureStreamContent, bool force = true);

	TBuilder AddByteArrayContent(ByteArrayContent byteArrayContent, bool force = true);

	TBuilder AddByteArrayContent(Func<ByteArrayContent, ByteArrayContent?> configureByteArrayContent, bool force = true);

}

public abstract class RequestBuilderBase<TBuilder, TObject> : IRequestBuilder<TBuilder, TObject>
	where TBuilder : RequestBuilderBase<TBuilder, TObject>
	where TObject : IHttpApiClientRequest
{
	protected readonly TBuilder _builder;
	protected TObject _request;

	protected RequestBuilderBase(TObject request)
	{
		_request = request;
		_builder = (TBuilder)this;
	}

	public virtual TBuilder Object(TObject request)
	{
		_request = request;
		return _builder;
	}

	public TObject Build()
		=> _request;

	public HttpRequestMessage ToHttpRequestMessage()
		=> _request.ToHttpRequestMessage();

	public TBuilder BaseAddress(string? baseAddress, bool force = true)
	{
		if (force || string.IsNullOrWhiteSpace(_request.BaseAddress))
			_request.BaseAddress = baseAddress;

		return _builder;
	}

	public TBuilder RelativePath(string path, bool force = true)
	{
		if (force || string.IsNullOrWhiteSpace(_request.RelativePath))
			_request.RelativePath = string.IsNullOrWhiteSpace(path)
				? ""
				: path.TrimPrefix("/");

		return _builder;
	}

	public TBuilder QueryString(string? queryString, bool force = true)
	{
		if (force || string.IsNullOrWhiteSpace(_request.QueryString))
			_request.QueryString = string.IsNullOrWhiteSpace(queryString)
				? null
				: $"?{queryString.TrimPrefix("?")}";

		return _builder;
	}

	public TBuilder QueryString(Dictionary<string, string>? queryString, bool force = true)
	{
		if (force || string.IsNullOrWhiteSpace(_request.QueryString))
		{
			if (queryString == null || queryString.Count == 0)
				_request.QueryString = null;
			else
				_request.QueryString = $"?{string.Join("&", queryString.Select(kvp => $"{System.Net.WebUtility.UrlEncode(kvp.Key)}={System.Net.WebUtility.UrlEncode(kvp.Value)}"))}";
		}

		return _builder;
	}

	public TBuilder AddQueryString(string key, string value)
	{
		if (string.IsNullOrWhiteSpace(key))
			return _builder;

		if (string.IsNullOrWhiteSpace(_request.QueryString))
		{
			QueryString(new Dictionary<string, string> { { key, value } });
		}
		else
		{
			_request.QueryString = $"{_request.QueryString}&{key}={value}";
		}

		return _builder;
	}

	public TBuilder Method(string httpMethod, bool force = true)
	{
		if (force || string.IsNullOrWhiteSpace(_request.HttpMethod))
		{
			if (string.IsNullOrWhiteSpace(httpMethod))
				throw new ArgumentNullException(nameof(httpMethod));

			_request.HttpMethod = httpMethod;
		}

		return _builder;
	}

	public TBuilder Method(HttpMethod httpMethod, bool force = true)
	{
		if (force || string.IsNullOrWhiteSpace(_request.HttpMethod))
		{
			if (httpMethod == null)
				throw new ArgumentNullException(nameof(httpMethod));

			_request.HttpMethod = httpMethod.Method;
		}

		return _builder;
	}

	public TBuilder ConfigureHeaders(Action<RequestHeaders> configure)
	{
		if (configure == null)
			return _builder;

		configure.Invoke(_request.Headers);

		return _builder;
	}

	public TBuilder Multipart(string multipartSubType, string? multipartBoundary)
	{
		if (string.IsNullOrWhiteSpace(multipartSubType))
			throw new ArgumentNullException(nameof(multipartSubType));

		_request.MultipartSubType = multipartSubType;
		_request.MultipartBoundary = multipartBoundary;
		return _builder;
	}

	public TBuilder RequestTimeout(TimeSpan? requestTimeout, bool force = true)
	{
		if (force || !_request.RequestTimeout.HasValue)
			_request.RequestTimeout = requestTimeout;

		return _builder;
	}

	public TBuilder AddFormData(KeyValuePair<string, string> formData, bool force = true)
	{
		if (force || _request.FormData == null || !_request.FormData.Any(x => x.Key == formData.Key))
		{
			_request.FormData ??= new List<KeyValuePair<string, string>>();
			_request.FormData.Add(formData);
		}

		return _builder;
	}

	public TBuilder AddFormData(List<KeyValuePair<string, string>> formData, bool force = true)
	{
		if (force || _request.FormData == null)
		{
			if (formData == null || formData.Count == 0)
				return _builder;

			_request.FormData ??= new List<KeyValuePair<string, string>>();
			_request.FormData.AddRange(formData);
		}

		return _builder;
	}

	public TBuilder AddStringContent(StringContent stringContent, bool force = true)
	{
		if (force || _request.StringContents == null)
		{
			if (stringContent == null)
				throw new ArgumentNullException(nameof(stringContent));

			_request.StringContents ??= new List<StringContent>();
			_request.StringContents.Add(stringContent);
		}

		return _builder;
	}

	public TBuilder AddStringContent(Func<StringContent, StringContent?> configureStringContent, bool force = true)
	{
		if (force || _request.StringContents == null)
		{
			if (configureStringContent == null)
				return _builder;

			_request.StringContents ??= new List<StringContent>();

			var stringContent = new StringContent();
			stringContent = configureStringContent.Invoke(stringContent);

			if (stringContent != null)
				_request.StringContents.Add(stringContent);
		}

		return _builder;
	}

	public TBuilder AddJsonContent(JsonContent jsonContent, bool force = true)
	{
		if (force || _request.JsonContents == null)
		{
			if (jsonContent == null)
				throw new ArgumentNullException(nameof(jsonContent));

			_request.JsonContents ??= new List<JsonContent>();
			_request.JsonContents.Add(jsonContent);
		}

		return _builder;
	}

	public TBuilder AddJsonContent(Func<JsonContent, JsonContent?> configureJsonContent, bool force = true)
	{
		if (force || _request.JsonContents == null)
		{
			if (configureJsonContent == null)
				return _builder;

			_request.JsonContents ??= new List<JsonContent>();

			var jsonContent = new JsonContent();
			jsonContent = configureJsonContent.Invoke(jsonContent);

			if (jsonContent != null)
				_request.JsonContents.Add(jsonContent);
		}

		return _builder;
	}

	public TBuilder AddJsonContent<T>(JsonContent<T> jsonContent, bool force = true)
	{
		if (force || _request.JsonContents == null)
		{
			if (jsonContent == null)
				throw new ArgumentNullException(nameof(jsonContent));

			_request.JsonContents ??= new List<JsonContent>();

			_request.JsonContents.Add(jsonContent);
		}

		return _builder;
	}

	public TBuilder AddJsonContent<T>(Func<JsonContent<T>, JsonContent<T>?> configureJsonContent, bool force = true)
	{
		if (force || _request.JsonContents == null)
		{
			if (configureJsonContent == null)
				return _builder;

			_request.JsonContents ??= new List<JsonContent>();

			var jsonContent = new JsonContent<T>();
			jsonContent = configureJsonContent.Invoke(jsonContent);

			if (jsonContent != null)
				_request.JsonContents.Add(jsonContent);
		}

		return _builder;
	}

	public TBuilder AddStreamContent(StreamContent streamContent, bool force = true)
	{
		if (force || _request.StreamContents == null)
		{
			if (streamContent == null)
				throw new ArgumentNullException(nameof(streamContent));

			_request.StreamContents ??= new List<StreamContent>();
			_request.StreamContents.Add(streamContent);
		}

		return _builder;
	}

	public TBuilder AddStreamContent(Func<StreamContent, StreamContent?> configureStreamContent, bool force = true)
	{
		if (force || _request.StreamContents == null)
		{
			if (configureStreamContent == null)
				return _builder;

			_request.StreamContents ??= new List<StreamContent>();

			var streamContent = new StreamContent();
			streamContent = configureStreamContent.Invoke(streamContent);

			if (streamContent != null)
				_request.StreamContents.Add(streamContent);
		}

		return _builder;
	}

	public TBuilder AddByteArrayContent(ByteArrayContent byteArrayContent, bool force = true)
	{
		if (force || _request.ByteArrayContents == null)
		{
			if (byteArrayContent == null)
				throw new ArgumentNullException(nameof(byteArrayContent));

			_request.ByteArrayContents ??= new List<ByteArrayContent>();

			_request.ByteArrayContents.Add(byteArrayContent);
		}

		return _builder;
	}

	public TBuilder AddByteArrayContent(Func<ByteArrayContent, ByteArrayContent?> configureByteArrayContent, bool force = true)
	{
		if (force || _request.ByteArrayContents == null)
		{
			if (configureByteArrayContent == null)
				return _builder;

			_request.ByteArrayContents ??= new List<ByteArrayContent>();

			var byteArrayContent = new ByteArrayContent();
			byteArrayContent = configureByteArrayContent.Invoke(byteArrayContent);

			if (byteArrayContent != null)
				_request.ByteArrayContents.Add(byteArrayContent);
		}

		return _builder;
	}
}

public class RequestBuilder : RequestBuilderBase<RequestBuilder, IHttpApiClientRequest>
{
	public RequestBuilder()
		: this(new HttpApiClientRequest())
	{
	}

	public RequestBuilder(IHttpApiClientRequest request)
		: base(request)
	{
	}

	public static implicit operator HttpApiClientRequest?(RequestBuilder builder)
	{
		if (builder == null)
			return null;

		return builder._request as HttpApiClientRequest;
	}

	public static implicit operator RequestBuilder?(HttpApiClientRequest request)
	{
		if (request == null)
			return null;

		return new RequestBuilder(request);
	}
}
