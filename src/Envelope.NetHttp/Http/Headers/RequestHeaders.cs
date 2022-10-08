using System.Net.Http.Headers;

namespace Envelope.NetHttp.Http.Headers;

public class RequestHeaders
{
	private const string Cookie = "Cookie";
	public List<string> Cookies { get; } = new List<string>();
	public List<ForceableKeyValuePair> CustomHeaders { get; } = new List<ForceableKeyValuePair>();
	public List<ForceableKeyValuePairList> CustomCollectionHeaders { get; } = new List<ForceableKeyValuePairList>();
	public List<ForceableKeyValuePair> CookieCollectionHeaders { get; } = new List<ForceableKeyValuePair>();

	public List<MediaTypeWithQualityHeader>? Accept { get; set; }
	public List<ProductInfoHeader>? UserAgent { get; set; }
	public List<ProductHeader>? Upgrade { get; set; }
	public bool? TransferEncodingChunked { get; set; }
	public List<TransferCodingHeader>? TransferEncoding { get; set; }
	public List<string>? Trailer { get; set; }
	public List<TransferCodingWithQualityHeader>? TE { get; set; }
	public string? Referrer { get; set; }
	public RangeHeader? Range { get; set; }
	public AuthenticationHeader? ProxyAuthorization { get; set; }
	public List<NameValueHeader>? Pragma { get; set; }
	public int? MaxForwards { get; set; }
	public DateTimeOffset? IfUnmodifiedSince { get; set; }
	public RangeConditionHeader? IfRange { get; set; }
	public List<ViaHeader>? Via { get; }
	public List<EntityTagHeader>? IfNoneMatch { get; }
	public List<EntityTagHeader>? IfMatch { get; }
	public string? Host { get; set; }
	public string? From { get; set; }
	public bool? ExpectContinue { get; set; }
	public List<NameValueWithParametersHeader>? Expect { get; }
	public DateTimeOffset? Date { get; set; }
	public bool? ConnectionClose { get; set; }
	public List<string>? Connection { get; }
	public CacheControlHeader? CacheControl { get; set; }
	public AuthenticationHeader? Authorization { get; set; }
	public List<StringWithQualityHeader>? AcceptLanguage { get; }
	public List<StringWithQualityHeader>? AcceptEncoding { get; }
	public List<StringWithQualityHeader>? AcceptCharset { get; }
	public DateTimeOffset? IfModifiedSince { get; set; }
	public List<WarningHeader>? Warning { get; set; }

	public RequestHeaders AddHeader(string name, string? value, bool force = true)
	{
		if (force || !CustomHeaders.Any(x => x.Key == name))
			CustomHeaders.Add(new ForceableKeyValuePair { Key = name, Value = value, Force = force });

		return this;
	}

	public RequestHeaders AddHeader(string name, IEnumerable<string?> values, bool force = true)
	{
		if (force || !CustomCollectionHeaders.Any(x => x.Key == name))
			CustomCollectionHeaders.Add(new ForceableKeyValuePairList { Key = name, Values = values, Force = force });

		return this;
	}

	public RequestHeaders AddCookie(string key, string value, bool force = true)
	{
		if (string.IsNullOrWhiteSpace(key))
			throw new ArgumentNullException(nameof(key));

		if (force || !CookieCollectionHeaders.Any(x => x.Key == key))
			CookieCollectionHeaders.Add(new ForceableKeyValuePair { Key = key, Value = value, Force = force } );

		return this;
	}

	public RequestHeaders AddCookie(string cookie)
	{
		if (string.IsNullOrWhiteSpace(cookie))
			return this;

		Cookies.Add(cookie);
		return this;
	}

	public void SetHttpRequestHeaders(HttpRequestHeaders httpRequestHeaders)
	{
		if (httpRequestHeaders == null)
			throw new ArgumentNullException(nameof(httpRequestHeaders));

		if (Accept != null && 0 < Accept.Count)
			foreach (var accept in Accept)
				httpRequestHeaders.Accept.Add(accept.ToMediaTypeWithQualityHeaderValue());

		if (UserAgent != null && 0 < UserAgent.Count)
			foreach (var userAgent in UserAgent)
				httpRequestHeaders.UserAgent.Add(userAgent.ToProductInfoHeaderValue());

		if (Upgrade != null && 0 < Upgrade.Count)
			foreach (var upgrade in Upgrade)
				httpRequestHeaders.Upgrade.Add(upgrade.ToProductHeaderValue());

		if (TransferEncodingChunked.HasValue)
			httpRequestHeaders.TransferEncodingChunked = TransferEncodingChunked;

		if (TransferEncoding != null && 0 < TransferEncoding.Count)
			foreach (var transferEncoding in TransferEncoding)
				httpRequestHeaders.TransferEncoding.Add(transferEncoding.ToTransferCodingHeaderValue());

		if (Trailer != null && 0 < Trailer.Count)
			foreach (var trailer in Trailer)
				httpRequestHeaders.Trailer.Add(trailer);

		if (TE != null && 0 < TE.Count)
			foreach (var te in TE)
				httpRequestHeaders.TE.Add(te.ToTransferCodingWithQualityHeaderValue());

		if (!string.IsNullOrWhiteSpace(Referrer))
				httpRequestHeaders.Referrer = new Uri(Referrer);

		if (Range != null)
			httpRequestHeaders.Range = Range.ToRangeHeaderValue();

		if (ProxyAuthorization != null)
			httpRequestHeaders.ProxyAuthorization = ProxyAuthorization.ToAuthenticationHeaderValue();

		if (Pragma != null && 0 < Pragma.Count)
			foreach (var pragma in Pragma)
				httpRequestHeaders.Pragma.Add(pragma.ToNameValueHeaderValue());

		if (MaxForwards.HasValue)
			httpRequestHeaders.MaxForwards = MaxForwards;

		if (IfUnmodifiedSince.HasValue)
			httpRequestHeaders.IfUnmodifiedSince = IfUnmodifiedSince;

		if (IfRange != null)
			httpRequestHeaders.IfRange = IfRange.ToRangeConditionHeaderValue();

		if (Via != null && 0 < Via.Count)
			foreach (var via in Via)
				httpRequestHeaders.Via.Add(via.ToViaHeaderValue());

		if (IfNoneMatch != null && 0 < IfNoneMatch.Count)
			foreach (var ifNoneMatch in IfNoneMatch)
				httpRequestHeaders.IfNoneMatch.Add(ifNoneMatch.ToEntityTagHeaderValue());

		if (IfMatch != null && 0 < IfMatch.Count)
			foreach (var ifMatch in IfMatch)
				httpRequestHeaders.IfMatch.Add(ifMatch.ToEntityTagHeaderValue());

		if (!string.IsNullOrWhiteSpace(Host))
			httpRequestHeaders.Host = Host;

		if (!string.IsNullOrWhiteSpace(From))
			httpRequestHeaders.From = From;

		if (ExpectContinue.HasValue)
			httpRequestHeaders.ExpectContinue = ExpectContinue;

		if (Expect != null && 0 < Expect.Count)
			foreach (var expect in Expect)
				httpRequestHeaders.Expect.Add(expect.ToNameValueWithParametersHeaderValue());

		if (Date.HasValue)
			httpRequestHeaders.Date = Date;

		if (ConnectionClose.HasValue)
			httpRequestHeaders.ConnectionClose = ConnectionClose;

		if (Connection != null && 0 < Connection.Count)
			foreach (var connection in Connection)
				httpRequestHeaders.Connection.Add(connection);

		if (CacheControl != null)
			httpRequestHeaders.CacheControl = CacheControl.ToCacheControlHeaderValue();

		if (Authorization != null)
			httpRequestHeaders.Authorization = Authorization.ToAuthenticationHeaderValue();

		if (AcceptLanguage != null && 0 < AcceptLanguage.Count)
			foreach (var acceptLanguage in AcceptLanguage)
				httpRequestHeaders.AcceptLanguage.Add(acceptLanguage.ToStringWithQualityHeaderValue());

		if (AcceptEncoding != null && 0 < AcceptEncoding.Count)
			foreach (var acceptEncoding in AcceptEncoding)
				httpRequestHeaders.AcceptEncoding.Add(acceptEncoding.ToStringWithQualityHeaderValue());

		if (AcceptCharset != null && 0 < AcceptCharset.Count)
			foreach (var acceptCharset in AcceptCharset)
				httpRequestHeaders.AcceptCharset.Add(acceptCharset.ToStringWithQualityHeaderValue());

		if (IfModifiedSince.HasValue)
			httpRequestHeaders.IfModifiedSince = IfModifiedSince;

		if (Warning != null && 0 < Warning.Count)
			foreach (var warning in Warning)
				httpRequestHeaders.Warning.Add(warning.ToWarningHeaderValue());

		foreach (var customHeader in CustomHeaders)
			if (customHeader.Force || !httpRequestHeaders.Contains(customHeader.Key))
			httpRequestHeaders.Add(customHeader.Key, customHeader.Value);

		foreach (var customCollectionHeader in CustomCollectionHeaders)
			if (customCollectionHeader.Force || !httpRequestHeaders.Contains(customCollectionHeader.Key))
				httpRequestHeaders.Add(customCollectionHeader.Key, customCollectionHeader.Values);

		if (0 < CookieCollectionHeaders.Count)
		{
			if (0 < Cookies?.Count)
			{
				httpRequestHeaders.Add(Cookie, $"{string.Join("; ", Cookies)};{string.Join("; ", CookieCollectionHeaders.Select(x => $"{x.Key}={x.Value}"))}");
			}
			else
			{
				httpRequestHeaders.Add(Cookie, string.Join("; ", CookieCollectionHeaders.Select(x => $"{x.Key}={x.Value}")));
			}
		}
	}
}
