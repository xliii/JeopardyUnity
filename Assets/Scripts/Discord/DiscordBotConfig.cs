using UnityEngine;

[CreateAssetMenu]
public class DiscordBotConfig : ScriptableObject
{
	public string token;
	public string textChannelID;
	public string voiceChannelID;
	public string guildID;		
}