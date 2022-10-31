namespace Envelope.NetHttp;

#nullable disable

public class ForceableKeyValuePair
{
	public string Key { get; set; }
	public string Value { get; set; }
	public bool Force { get; set; }
}

public class ForceableKeyValuePairList
{
	public string Key { get; set; }
	public IEnumerable<string> Values { get; set; }
	public bool Force { get; set; }
}
