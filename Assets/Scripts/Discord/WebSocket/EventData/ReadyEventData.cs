using System.Collections.Generic;

public class ReadyEventData : IGatewayEventData
{

	public const string Name = "READY";
	
	//TODO: User settings
	public User user;
	public string session_id;
	//TODO: Relationships, private channels, presences

	public List<Guild> guilds;
	public List<string> _trace;
}

public class Guild
{
	public bool unavailable;
	public string id;
}
