public class VoiceServerUpdate : IGatewayEventData
{
	public const string Name = "VOICE_SERVER_UPDATE";

	public string token;
	public string guild_id;
	public string endpoint;
}
