using System.Net.Http.Headers;

namespace Envelope.NetHttp.Http.Headers;

public class CacheControlHeader
{
	public bool? ProxyRevalidate { get; set; }
	public List<string>? PrivateHeaders { get; }
	public bool? Private { get; set; }
	public bool? OnlyIfCached { get; set; }
	public bool? NoTransform { get; set; }
	public bool? NoStore { get; set; }
	public List<string>? NoCacheHeaders { get; }
	public bool? NoCache { get; set; }
	public bool? MustRevalidate { get; set; }
	public TimeSpan? MinFresh { get; set; }
	public TimeSpan? MaxStaleLimit { get; set; }
	public bool? MaxStale { get; set; }
	public TimeSpan? MaxAge { get; set; }
	public List<NameValueHeader>? Extensions { get; }
	public bool? Public { get; set; }
	public TimeSpan? SharedMaxAge { get; set; }

	public CacheControlHeaderValue ToCacheControlHeaderValue()
	{
		var cacheControlHeaderValue = new CacheControlHeaderValue();

		if (ProxyRevalidate.HasValue)
			cacheControlHeaderValue.ProxyRevalidate = ProxyRevalidate.Value;

		if (PrivateHeaders != null && 0 < PrivateHeaders.Count)
			foreach (var privateHeader in PrivateHeaders)
				cacheControlHeaderValue.PrivateHeaders.Add(privateHeader);

		if (Private.HasValue)
			cacheControlHeaderValue.Private = Private.Value;

		if (OnlyIfCached.HasValue)
			cacheControlHeaderValue.OnlyIfCached = OnlyIfCached.Value;

		if (NoTransform.HasValue)
			cacheControlHeaderValue.NoTransform = NoTransform.Value;

		if (NoStore.HasValue)
			cacheControlHeaderValue.NoStore = NoStore.Value;

		if (NoCacheHeaders != null && 0 < NoCacheHeaders.Count)
			foreach (var noCacheHeader in NoCacheHeaders)
				cacheControlHeaderValue.NoCacheHeaders.Add(noCacheHeader);

		if (NoCache.HasValue)
			cacheControlHeaderValue.NoCache = NoCache.Value;

		if (MustRevalidate.HasValue)
			cacheControlHeaderValue.MustRevalidate = MustRevalidate.Value;

		if (MinFresh.HasValue)
			cacheControlHeaderValue.MinFresh = MinFresh.Value;

		if (MaxStaleLimit.HasValue)
			cacheControlHeaderValue.MaxStaleLimit = MaxStaleLimit.Value;

		if (MaxStale.HasValue)
			cacheControlHeaderValue.MaxStale = MaxStale.Value;

		if (MaxAge.HasValue)
			cacheControlHeaderValue.MaxAge = MaxAge.Value;

		if (Extensions != null && 0 < Extensions.Count)
			foreach (var extension in Extensions)
				cacheControlHeaderValue.Extensions.Add(extension.ToNameValueHeaderValue());

		if (Public.HasValue)
			cacheControlHeaderValue.Public = Public.Value;

		if (SharedMaxAge.HasValue)
			cacheControlHeaderValue.SharedMaxAge = SharedMaxAge.Value;

		return cacheControlHeaderValue;
	}
}
