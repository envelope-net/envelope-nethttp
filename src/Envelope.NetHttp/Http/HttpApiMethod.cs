namespace Envelope.NetHttp.Http;

public enum HttpApiMethod
{
	Get = 0,
	Post = 1,
	Put = 2,
	Delete = 3,
	Options = 4,
	Head = 5,
	Trace = 6,
#if NET6_0_OR_GREATER
	Patch = 7
#endif
}

public static class HttpApiMethodExtensions
{
	public static HttpMethod ToHttpMethod(this HttpApiMethod httpApiMethod)
	{
		return httpApiMethod switch
		{
			HttpApiMethod.Get => HttpMethod.Get,
			HttpApiMethod.Post => HttpMethod.Post,
			HttpApiMethod.Put => HttpMethod.Put,
			HttpApiMethod.Delete => HttpMethod.Delete,
			HttpApiMethod.Options => HttpMethod.Options,
			HttpApiMethod.Head => HttpMethod.Head,
			HttpApiMethod.Trace => HttpMethod.Trace,
#if NET6_0_OR_GREATER
			HttpApiMethod.Patch => HttpMethod.Patch,
#endif
			_ => HttpMethod.Get,
		};
	}
}
