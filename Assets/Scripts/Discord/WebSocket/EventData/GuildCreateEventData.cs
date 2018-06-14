using System.Collections.Generic;

public class GuildCreateEventData : IGatewayEventData
{

	public const string Name = "GUILD_CREATE";
	public bool unavailable;
	//TODO: system_channel_id & splash
	public List<Role> roles;
	public string region;
	//TODO: Presences
	public string owner_id;
	public string name;
	public int mfa_level;
	public List<Member> members;
	public int member_count;
	public bool lazy;
	public bool large;
	public string joined_at; //TODO: timestamp
	public string id;
	public string icon;
	//TODO: features
	public int explicit_content_filter;
	//TODO: emojis
	public int default_message_notifications;
	public List<Channel> channels;
	public int afk_timeout;
	//TODO: afk_channel_id & application_id
}

public class Channel //TODO: Polymorphism support
{
	public int type; //0 - text, 2 - voice
	public string topic; //text only
	public int position;
	//TODO: permission_overwrites
	public string name;
	public string id;
	public string last_message_id; //text only
	public int? bitrate; //voice only
	public int? user_limit; //voice only;
}

public class Role
{
	public int position;
	public int permissions;
	public string name;
	public bool mentionable;
	public bool managed;
	public string id;
	public bool hoist;
	public int color;
}