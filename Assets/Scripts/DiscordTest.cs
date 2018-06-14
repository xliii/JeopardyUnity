using Discord;
using UnityEngine;
using UnityEngine.UI;

public class DiscordTest : MonoBehaviour
{

	public InputField input;

	public DiscordBotConfig config;

	private DiscordClient client;
	
	// Use this for initialization
	void Start () {
		client = new DiscordBotClient(config);		
		var response = client.Channel(config.channelID).AddMessage("Hello world");
		Debug.Log($"Message added: {response.content}");
		var botGateway = client.Gateway().GetBotGateway().url;
		Debug.Log($"Bot gateway: {botGateway}");
	}

	void OnApplicationQuit()
	{
		client.Dispose();
	}		

	public void SendDiscordMessage()
	{
		var message = input.text;
		var response = client.Channel(config.channelID).AddMessage(message);
		Debug.Log($"Message added: {response.content}");
	}
}
