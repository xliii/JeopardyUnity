using Newtonsoft.Json;

public class IdentifyEventData
{

	public string token;
	public ConnectionProperties properties;
	public bool? compress;
	public int? large_treshold;
	public int[] shard;
	//TODO: Presence 

}

public class ConnectionProperties
{
	[JsonProperty(PropertyName = "$os")]
	public string os;
	
	[JsonProperty(PropertyName = "$browser")]
	public string browser;
	
	[JsonProperty(PropertyName = "$device")]
	public string device;
}
