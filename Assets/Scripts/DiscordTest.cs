using Discord;
using UnityEngine;
using UnityEngine.UI;

public class DiscordTest : MonoBehaviour
{
	public InputField input;

	public DiscordBotConfig config;

	private DiscordClient client;
	
	public Button sendButton;

	public Song song;
	
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

	public void Play()
	{
		client.voiceClient.Play(song);
	}

	public void Stop()
	{
		client.voiceClient.Stop();
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
