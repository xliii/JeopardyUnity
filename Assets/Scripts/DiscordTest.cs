using System.Diagnostics;
using System.IO;
using Discord;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

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
		string path = $"{Application.dataPath}/Music/Test.wav";
		if (!File.Exists(path))
		{
			Debug.LogError("AudioFile not found");
			return;
		}
		
		Debug.Log("Found audio file");
		
		using (var ffmpeg = CreateFFmpeg(path))
		{
			using (var stream = client.CreatePCMStream(AudioApplication.Music))
			{
				try
				{
					ffmpeg.StandardOutput.BaseStream.CopyTo(stream);
				}
				finally
				{
					stream.Flush();
				}
			}
		}
	}

	private Process CreateFFmpeg(string path)
	{
		return Process.Start(new ProcessStartInfo
		{
			FileName = $"{Application.dataPath}/ffmpeg/ffmpeg.exe",
			Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			CreateNoWindow = true
		});
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
