using Discord;
using UnityEngine;

public class DiscordTest : MonoBehaviour
{

	public DiscordBotConfig config;

	private DiscordClient client;
	
	// Use this for initialization
	void Start () {
		client = new DiscordBotClient(config);
		var response = client.Channel(config.channelID).AddMessage("Hello world");
		Debug.Log($"Message added: {response}");
		var botGateway = client.Gateway().GetBotGateway().url;
		Debug.Log($"Bot gateway: {botGateway}");
	}

	void OnApplicationQuit()
	{
		client.Dispose();
	}	
}
