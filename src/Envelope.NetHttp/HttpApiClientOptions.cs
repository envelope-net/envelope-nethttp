using Envelope.NetHttp.Http;
using Envelope.Policy;
using Envelope.Security.Cryptography;
using Envelope.Text;
using Envelope.Validation;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Envelope.NetHttp;

public abstract class HttpApiClientOptions : IValidable
{
	private Type? _defaultRequestResponseLoggerType;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	public string ClientName { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	public string SourceSystemName { get; set; } = nameof(HttpApiClient);
	public string? BaseAddress { get; set; }
	public string? UserAgent { get; set; } = nameof(HttpApiClient);
	public Version? Version { get; set; }
	public Dictionary<string, string>? StaticQueryStrings { get; set; }
	public bool ForceStaticQueryStrings { get; set; }
	public List<ForceableKeyValuePair>? StaticHeaders { get; set; }
	public List<ForceableKeyValuePairList>? StaticHeaderCollections { get; set; }
	public List<ForceableKeyValuePair>? StaticCookies { get; set; }
	public List<ForceableKeyValuePair>? StaticFormData { get; set; }

	#region HttpClientHandler

	public DecompressionMethods? AutomaticDecompression { get; set; } = DecompressionMethods.GZip;
	public IWebProxy? Proxy { get; set; }
	public bool? UseProxy { get; set; }
	public ICredentials? DefaultProxyCredentials { get; set; }
	public bool TrustToAllServerCertificates { get; set; }
	public bool? CheckCertificateRevocationList { get; set; }
	public bool UsesCookieContainerToStoreServerCookies { get; set; }
	public bool? UseDefaultCredentials { get; set; }
	public CredentialCache? CredentialCache { get; set; }
	public ICredentials? Credentials { get; set; }
	public List<X509Certificate>? ClientCertificates { get; set; }
	public bool SendAuthorizationHeaderInRequest { get; set; }
	public SslProtocols? SslProtocols { get; set; }
	public int? MaxResponseHeadersLength { get; set; }
	public long? MaxRequestContentBufferSize { get; set; }
	public int? MaxConnectionsPerServer { get; set; }
	public int? MaxAutomaticRedirections { get; set; }
	public bool? AllowAutoRedirect { get; set; }

	#endregion HttpClientHandler

	public Dictionary<string, IAsyncPolicy<HttpResponseMessage>>? UriPolicies { get; set; } //Dictionary<Uri, IAsyncPolicy<HttpResponseMessage>> OR ----- Wildcard ----- Dictionary<*, IAsyncPolicy<HttpResponseMessage>>
	public List<string>? LogDisabledUris { get; set; }
	public Dictionary<string, IRequestResponseLogger>? UriLoggers { get; set; } //Dictionary<Uri, IRequestResponseLogger> OR ----- Wildcard ----- Dictionary<*, IRequestResponseLogger>

	//Func<object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors, bool)>
	public Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool>? RemoteCertificateValidationCallback { get; set; }
		= DefaultServerCertificateValidation.ServerCertificateValidation;

	public void SetDefaultRequestResponseLogger<T>()
		where T : IRequestResponseLogger
	{
		_defaultRequestResponseLoggerType = typeof(T);
	}

	public bool ApplyToHttpClientHandler => 
		AutomaticDecompression.HasValue
		|| Proxy != null
		|| UseProxy.HasValue
		|| DefaultProxyCredentials != null
		|| TrustToAllServerCertificates
		|| CheckCertificateRevocationList.HasValue
		|| !UsesCookieContainerToStoreServerCookies
		|| UseDefaultCredentials.HasValue
		|| CredentialCache != null
		|| Credentials != null
		|| 0 < ClientCertificates?.Count
		|| SendAuthorizationHeaderInRequest
		|| SslProtocols.HasValue
		|| MaxResponseHeadersLength.HasValue
		|| MaxRequestContentBufferSize.HasValue
		|| MaxConnectionsPerServer.HasValue
		|| MaxAutomaticRedirections.HasValue
		|| AllowAutoRedirect.HasValue
		;

	public void ConfigureHttpClientHandler(HttpClientHandler handler)
	{
		if (handler == null)
			throw new ArgumentNullException(nameof(handler));

		var isBrowser = Envelope.Infrastructure.OSPlatformHelper.IsBrowser();

		if (!isBrowser)
		{
			if (AutomaticDecompression.HasValue)
				handler.AutomaticDecompression = AutomaticDecompression.Value;

			if (Proxy != null)
				handler.Proxy = Proxy;

			if (UseProxy.HasValue)
				handler.UseProxy = UseProxy.Value;

			if (DefaultProxyCredentials != null)
				handler.DefaultProxyCredentials = DefaultProxyCredentials;

			if (TrustToAllServerCertificates && RemoteCertificateValidationCallback != null)
				handler.ServerCertificateCustomValidationCallback = RemoteCertificateValidationCallback!;

			if (CheckCertificateRevocationList.HasValue)
				handler.CheckCertificateRevocationList = CheckCertificateRevocationList.Value;

			if (!UsesCookieContainerToStoreServerCookies)
				handler.UseCookies = false;

			if (UseDefaultCredentials.HasValue)
				handler.UseDefaultCredentials = UseDefaultCredentials.Value;

			if (CredentialCache != null)
				handler.Credentials = CredentialCache;
			else if (Credentials != null)
				handler.Credentials = Credentials;

			if (0 < ClientCertificates?.Count)
				handler.ClientCertificates.AddRange(ClientCertificates.ToArray());

			if (SendAuthorizationHeaderInRequest)
				handler.PreAuthenticate = true;

			if (SslProtocols.HasValue)
				handler.SslProtocols = SslProtocols.Value;

			if (MaxResponseHeadersLength.HasValue)
				handler.MaxResponseHeadersLength = MaxResponseHeadersLength.Value;

			if (MaxConnectionsPerServer.HasValue)
				handler.MaxConnectionsPerServer = MaxConnectionsPerServer.Value;

			if (MaxAutomaticRedirections.HasValue)
				handler.MaxAutomaticRedirections = MaxAutomaticRedirections.Value;
		}

		if (MaxRequestContentBufferSize.HasValue)
			handler.MaxRequestContentBufferSize = MaxRequestContentBufferSize.Value;

		if (AllowAutoRedirect.HasValue)
			handler.AllowAutoRedirect = AllowAutoRedirect.Value;
	}

	public void ConfigureStaticRequestParams(IHttpApiClientRequest request)
	{
		if (request == null)
			throw new ArgumentNullException(nameof(request));

		var builder = new RequestBuilder(request);

		if (StaticQueryStrings != null)
		{
			var dict = new Dictionary<string, string>();
			foreach (var staticQueryString in StaticQueryStrings)
				if (!string.IsNullOrWhiteSpace(staticQueryString.Key))
					dict[staticQueryString.Key] = staticQueryString.Value;

			if (0 < dict.Count)
				builder.QueryString(dict, ForceStaticQueryStrings);
		}

		if (StaticHeaders != null)
		{
			foreach (var staticHeader in StaticHeaders)
			{
				if (string.IsNullOrWhiteSpace(staticHeader.Key))
					continue;

				request.Headers.AddHeader(staticHeader.Key, staticHeader.Value, staticHeader.Force);
			}
		}

		if (StaticHeaderCollections != null)
		{
			foreach (var staticHeaderCollection in StaticHeaderCollections)
			{
				if (string.IsNullOrWhiteSpace(staticHeaderCollection.Key))
					continue;

				request.Headers.AddHeader(staticHeaderCollection.Key, staticHeaderCollection.Values, staticHeaderCollection.Force);
			}
		}

		if (StaticCookies != null)
		{
			foreach (var staticCookie in StaticCookies)
			{
				if (string.IsNullOrWhiteSpace(staticCookie.Key))
					continue;

				request.Headers.AddCookie(staticCookie.Key, staticCookie.Value, staticCookie.Force);
			}
		}

		if (StaticFormData != null)
		{
			foreach (var staticFormData in StaticFormData)
			{
				if (string.IsNullOrWhiteSpace(staticFormData.Key))
					continue;

				builder.AddFormData(new KeyValuePair<string, string>(staticFormData.Key, staticFormData.Value), staticFormData.Force);
			}
		}
	}

	public void AddCredentialCache(string host, int port, AuthenticationType authenticationType, NetworkCredential credential)
	{
		if (string.IsNullOrWhiteSpace(host))
			throw new ArgumentNullException(nameof(host));

		if (credential == null)
			throw new ArgumentNullException(nameof(credential));

		CredentialCache ??= new CredentialCache();
		CredentialCache.Add(host, port, authenticationType.ToString(), credential);
	}

	public void AddCredentialCache(string uriPrefix, AuthenticationType authenticationType, NetworkCredential credential)
	{
		if (string.IsNullOrWhiteSpace(uriPrefix))
			throw new ArgumentNullException(nameof(uriPrefix));

		if (credential == null)
			throw new ArgumentNullException(nameof(credential));

		CredentialCache ??= new CredentialCache();
		CredentialCache.Add(new Uri(uriPrefix), authenticationType.ToString(), credential);
	}

	public List<IValidationMessage>? Validate(string? propertyPrefix = null, List<IValidationMessage>? parentErrorBuffer = null, Dictionary<string, object>? validationContext = null)
	{
		//if (string.IsNullOrWhiteSpace(BaseAddress))
		//{
		//	parentErrorBuffer ??= new List<IValidationMessage>();
		//	parentErrorBuffer.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(BaseAddress))} == null"));
		//}

		if (string.IsNullOrWhiteSpace(ClientName))
		{
			parentErrorBuffer ??= new List<IValidationMessage>();
			parentErrorBuffer.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(ClientName))} == null"));
		}

		if (Credentials != null && CredentialCache != null)
		{
			parentErrorBuffer ??= new List<IValidationMessage>();
			parentErrorBuffer.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(Credentials))} != null && {StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(CredentialCache))} != null"));
		}

		return parentErrorBuffer;
	}

	public virtual IRequestResponseLogger? GetLogger(string? uri, IServiceProvider? serviceProvider = null)
	{
		if (serviceProvider != null)
		{
			if (_defaultRequestResponseLoggerType != null)
			{
				var requestResponseLogger = serviceProvider.GetService(_defaultRequestResponseLoggerType);
				if (requestResponseLogger != null)
					return (IRequestResponseLogger)requestResponseLogger;
			}
			else
			{
				var requestResponseLogger = serviceProvider.GetService<IRequestResponseLogger>();
				if (requestResponseLogger != null)
					return requestResponseLogger;
			}
		}

		if (string.IsNullOrWhiteSpace(uri))
			return null;

		if (LogDisabledUris != null && LogDisabledUris.Any(x => uri!.StartsWith(x)))
			return null;

		if (UriLoggers == null || UriLoggers.Count == 0)
			return null;

		var key = UriLoggers.Keys.FirstOrDefault(x => uri!.StartsWith(x));
		if (!string.IsNullOrWhiteSpace(key) && UriLoggers.TryGetValue(key, out var logger))
			return logger;

		if (UriLoggers.TryGetValue("*", out var defaultLogger))
			return defaultLogger;

		return null;
	}
}
