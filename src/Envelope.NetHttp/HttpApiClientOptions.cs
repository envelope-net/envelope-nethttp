using Envelope.Policy;
using Envelope.Security.Cryptography;
using Envelope.Text;
using Envelope.Validation;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Envelope.NetHttp;

public abstract class HttpApiClientOptions : IValidable
{
	public string SourceSystemName { get; set; } = nameof(HttpApiClient);
	public string? BaseAddress { get; set; }
	public string? UserAgent { get; set; } = nameof(HttpApiClient);
	public Version? Version { get; set; }

	#region HttpClientHandler

	public DecompressionMethods? AutomaticDecompression { get; set; } = DecompressionMethods.GZip;
	public IWebProxy? Proxy { get; set; }
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

	public bool ApplyToHttpClientHandler => 
		AutomaticDecompression.HasValue
		|| Proxy != null
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

		if (AutomaticDecompression.HasValue)
			handler.AutomaticDecompression = AutomaticDecompression.Value;

		if (Proxy != null)
			handler.Proxy = Proxy;

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

		if (MaxRequestContentBufferSize.HasValue)
			handler.MaxRequestContentBufferSize = MaxRequestContentBufferSize.Value;

		if (MaxConnectionsPerServer.HasValue)
			handler.MaxConnectionsPerServer = MaxConnectionsPerServer.Value;

		if (MaxAutomaticRedirections.HasValue)
			handler.MaxAutomaticRedirections = MaxAutomaticRedirections.Value;

		if (AllowAutoRedirect.HasValue)
			handler.AllowAutoRedirect = AllowAutoRedirect.Value;
	}

	public void AddCredentialCache(string host, int port, AuthenticationType authenticationType, NetworkCredential credential)
	{
		if (string.IsNullOrWhiteSpace(host))
			throw new ArgumentNullException(nameof(host));

		if (credential == null)
			throw new ArgumentNullException(nameof(credential));

		if (CredentialCache == null)
			CredentialCache = new CredentialCache();

		CredentialCache.Add(host, port, authenticationType.ToString(), credential);
	}

	public void AddCredentialCache(string uriPrefix, AuthenticationType authenticationType, NetworkCredential credential)
	{
		if (string.IsNullOrWhiteSpace(uriPrefix))
			throw new ArgumentNullException(nameof(uriPrefix));

		if (credential == null)
			throw new ArgumentNullException(nameof(credential));

		if (CredentialCache == null)
			CredentialCache = new CredentialCache();

		CredentialCache.Add(new Uri(uriPrefix), authenticationType.ToString(), credential);
	}

	public List<IValidationMessage>? Validate(string? propertyPrefix = null, List<IValidationMessage>? parentErrorBuffer = null, Dictionary<string, object>? validationContext = null)
	{
		if (string.IsNullOrWhiteSpace(BaseAddress))
		{
			if (parentErrorBuffer == null)
				parentErrorBuffer = new List<IValidationMessage>();

			parentErrorBuffer.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(BaseAddress))} == null"));
		}

		if (Credentials != null && CredentialCache != null)
		{
			if (parentErrorBuffer == null)
				parentErrorBuffer = new List<IValidationMessage>();

			parentErrorBuffer.Add(ValidationMessageFactory.Error($"{StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(Credentials))} != null && {StringHelper.ConcatIfNotNullOrEmpty(propertyPrefix, ".", nameof(CredentialCache))} != null"));
		}

		return parentErrorBuffer;
	}
}
