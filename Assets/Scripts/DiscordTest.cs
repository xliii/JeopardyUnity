using Discord;
using UnityEngine;
using UnityEngine.UI;
using UnityParseHelpers;

public class DiscordTest : MonoBehaviour
{
	public InputField input;

	public DiscordBotConfig config;

	private DiscordClient client;

	public AudioClip audioClip;

	public Button sendButton;
	
	// Use this for initialization
	void Start ()
	{
		sendButton.interactable = false;
		
		client = new DiscordBotClient(config);
		client.OnReady += OnReady;
		client.OnMessage += OnMessage;
		client.OnVoiceReady += OnVoiceReady;
		client.Connect();
	}

	public void SendVoice()
	{
		client.SendVoice(audioClip);
	}

	private void OnVoiceReady(object sender, SessionDesciptionResponse e)
	{
		sendButton.interactable = true;
	}

	private void OnMessage(object sender, MessageCreateEventData e)
	{
		Debug.Log($"-- {e.author.username}: {e.content}");
	}

	private void OnReady(object sender, ReadyEventData e)
	{
		client.JoinVoice(config.guildID, config.voiceChannelID);
	}

	void OnApplicationQuit()
	{
		client.Dispose();
	}		

	public void SendDiscordMessage()
	{
		var message = input.text;
		var response = client.Channel(config.textChannelID).AddMessage(message);
		Debug.Log($"Message added: {response.content}");
	}
}
