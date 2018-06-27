using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord;
using UnityEngine;
using UnityEngine.UI;
using UnityParseHelpers;
using Debug = UnityEngine.Debug;

public class DiscordTest : MonoBehaviour
{
	public InputField input;

	public DiscordBotConfig config;

	private DiscordClient client;

	public AudioClip audioClip;

	public Button sendButton;

	private string appDataPath;
	
	// Use this for initialization
	void Start ()
	{
		sendButton.interactable = false;
		
		client = new DiscordBotClient(config);
		client.OnReady += OnReady;
		client.OnMessage += OnMessage;
		client.OnVoiceReady += OnVoiceReady;
		client.Connect();
		appDataPath = Application.dataPath;
	}

	public void SendVoice()
	{
		Loom.Instance.RunAsync(SendVoiceAsync);
	}

	public async void SendVoiceAsync()
	{
		string path = $"{appDataPath}/Music/Never gonna give you up.wav";
		if (!File.Exists(path))
		{
			Debug.LogError("AudioFile not found");
			return;
		}
		
		Debug.Log("Found audio file");

		await PCM(path);
	}
	
	private async Task PCM(string path)
	{
		using (var ffmpeg = CreateFFmpeg(path))
		{
			using (var stream = client.CreatePCMStream(AudioApplication.Music))
			{
				try
				{
					await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream);
				}
				finally
				{
					await stream.FlushAsync();
				}		
			}
		}
	}

	private async Task DirectPCM(string path)
	{
		using (var ffmpeg = CreateFFmpeg(path))
		{
			using (var stream = client.CreateDirectPCMStream(AudioApplication.Music))
			{
				try
				{
					client.voiceClient.ToggleSpeaking(true);
					await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream);
				}
				finally
				{
					await stream.FlushAsync();
					client.voiceClient.ToggleSpeaking(false);
				}		
			}
		}
	}

	private Process CreateFFmpeg(string path)
	{
		return Process.Start(new ProcessStartInfo
		{
			FileName = $"{appDataPath}/ffmpeg/ffmpeg.exe",
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
