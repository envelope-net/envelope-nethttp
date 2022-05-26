using System.Net.Http.Headers;

namespace Envelope.NetHttp.Http.Headers;

public class NameValueWithParametersHeader : NameValueHeader
{
	public List<NameValueHeader>? Parameters { get; set; }

	public NameValueWithParametersHeaderValue ToNameValueWithParametersHeaderValue()
	{
		if (string.IsNullOrWhiteSpace(Name))
			throw new InvalidOperationException($"{nameof(Name)} == null");

		var nameValueWithParametersHeaderValue =
			string.IsNullOrWhiteSpace(Value)
				? new NameValueWithParametersHeaderValue(Name)
				: new NameValueWithParametersHeaderValue(Name, Value);

		if (Parameters != null && 0 < Parameters.Count)
			foreach (var parameter in Parameters)
				nameValueWithParametersHeaderValue.Parameters.Add(parameter.ToNameValueHeaderValue());

		return nameValueWithParametersHeaderValue;
	}
}
