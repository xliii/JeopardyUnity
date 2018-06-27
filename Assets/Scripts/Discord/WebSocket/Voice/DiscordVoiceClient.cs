using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityParseHelpers;
using Debug = UnityEngine.Debug;

public class DiscordVoiceClient : IDisposable
{
	private DiscordGatewayClient gateway;

	private string guildId;
	private string userId;
	private string token;
	private string sessionId;
	private string endpoint;

	private DiscordVoiceGatewayClient voiceGateway;

	private IHeartbeatService heartbeatService;

	private VoiceUdpClient udpClient;

	public byte[] SecretKey => udpClient.SecretKey;

	public event EventHandler<SessionDesciptionResponse> OnVoiceReady;

	private CancellationToken _cancellationToken;
	private CancellationTokenSource _cancellationTokenSource;

	private bool speaking = false;

	private Process ffmpegProcess;
	private AudioOutStream audioStream;
	
	public DiscordVoiceClient(string userId, DiscordGatewayClient gateway, CancellationToken cancellationToken)
	{
		_cancellationToken = cancellationToken;
		this.gateway = gateway;
		this.userId = userId;
		
		//Voice initialization
		Messenger.AddListener<VoiceServerUpdate>(DiscordEvent.Voice.ServerUpdate, OnServerUpdate);
		Messenger.AddListener<VoiceStateUpdateResponse>(DiscordEvent.Voice.StatusUpdate, OnStatusUpdate);
		Messenger.AddListener<HelloEventData>(DiscordEvent.Voice.Hello, OnHello);
		Messenger.AddListener<VoiceReadyResponse>(DiscordEvent.Voice.Ready, OnReady);
		Messenger.AddListener<SessionDesciptionResponse>(DiscordEvent.Voice.SessionDesciption, OnSessionDescription);
		//Messenger.AddListener<SpeakingResponse>(DiscordEvent.Voice.Speaking, OnSpeaking);
		Messenger.AddListener<byte[]>(DiscordEvent.Voice.Packet, OnPacket);
	}

	private void OnPacket(byte[] packet)
	{
		try
		{
			if (packet.Length == 70)
			{
				DiscoverIp(packet);
			}
		}
		catch (Exception e)
		{
			Debug.LogError($"Error while processing UDP Packet: {e}");
		}
	}

	private void DiscoverIp(byte[] packet)
	{
		string ip = Encoding.UTF8.GetString(packet, 4, 70 - 6).TrimEnd('\0');
		int port = (packet[69] << 8) | packet[68];
		Debug.Log($"IP Discovered: {ip}:{port}");
		
		var payload = new GatewayPayload
		{
			OpCode = GatewayOpCode.Voice_SelectProtocol,
			Data = new SelectProtocolRequest
			{
				protocol = "udp",
				data = new ProtocolData
				{
					address = ip,
					port = port,
					mode = udpClient.Mode
				}
			}
		};
		voiceGateway.Send(payload);
	}

	private void OnSessionDescription(SessionDesciptionResponse e)
	{
		udpClient.SecretKey = e.secret_key;
		OnVoiceReady.Invoke(this, e);
	}

	public void ToggleSpeaking(bool speaking)
	{
		if (this.speaking == speaking) return;

		this.speaking = speaking;
		
		var payload = new GatewayPayload
		{
			OpCode = GatewayOpCode.Voice_Speaking,
			Data = new SpeakingRequest
			{
				speaking = speaking,
				delay = 0,
				ssrc = udpClient.ssrc
			}
		};
		
		voiceGateway.Send(payload);
	}

	private void OnReady(VoiceReadyResponse e)
	{
		//Start heartbeat
		heartbeatService.Start();
		//Initialize UDP
		udpClient = new VoiceUdpClient(e.ip, e.port, e.ssrc);
		udpClient.Start();
	}
	
	//TODO: Handle Resume:
	///<summary>	
	///See <a href="https://discordapp.com/developers/docs/topics/voice-connections#resuming-voice-connection">Discord API Documentation</a>
	///</summary>
	///

	private void OnHello(HelloEventData e)
	{
		var payload = new GatewayPayload
		{
			OpCode = GatewayOpCode.Voice_Identify,
			Data = new VoiceIdentifyRequest
			{
				server_id = guildId,
				user_id = userId,
				session_id = sessionId,
				token = token
			}
		};
					
		voiceGateway.Send(payload);
		heartbeatService = new VoiceHeartbeatService(voiceGateway, e.heartbeat_interval);
	}
	
	public void JoinVoice(string guildId, string channelId)
	{
		var payload = new GatewayPayload
		{
			OpCode = GatewayOpCode.VoiceStateUpdate,
			Data = new VoiceStateUpdateRequest
			{
				guild_id = guildId,
				channel_id = channelId,
				self_mute = false,
				self_deaf = false
			}
		};
            
		gateway.Send(payload);
	}
	
	
	private void OnStatusUpdate(VoiceStateUpdateResponse e)
	{
		sessionId = e.session_id;
		TryConnect();
	}

	private void OnServerUpdate(VoiceServerUpdate e)
	{
		endpoint = $"ws://{e.endpoint}";
		guildId = e.guild_id;
		token = e.token;
		TryConnect();
	}

	private void TryConnect()
	{
		if (token == null || guildId == null || endpoint == null || sessionId == null)
		{
			return;
		}
		
		voiceGateway = new DiscordVoiceGatewayClient(endpoint);
	}

	public void Dispose()
	{
		voiceGateway?.Dispose();
		heartbeatService?.Dispose();
		udpClient?.Dispose();
	}

	private AudioOutStream CreateStream(AudioApplication application, int? bitrate = null, int bufferMillis = 1000, int packetLoss = 30)
	{
		var outputStream = new OutputStream(udpClient); //Ignores header
		var sodiumEncrypter = new SodiumEncryptStream(outputStream, this); //Passes header
		var rtpWriter = new RTPWriteStream(sodiumEncrypter, udpClient.ssrc); //Consumes header, passes
		var bufferedStream = new BufferedWriteStream(rtpWriter, this, bufferMillis, _cancellationToken); //Ignores header, generates header
		return new OpusEncodeStream(bufferedStream, bitrate ?? (96 * 1024), application, packetLoss); //Generates header
	}

	public void Play(Song song)
	{
		Loom.Instance.RunAsync(() => { PlayAsync(song); });
	}

	public async Task PlayAsync(Song song)
	{
		Debug.Log("PLAY");
		if (!File.Exists(song.Filepath))
		{
			Debug.LogError("AudioFile not found");
			return;
		}
		
		using (ffmpegProcess = CreateFFmpeg(song.Filepath))
		{
			if (audioStream == null)
			{
				audioStream = CreateStream(AudioApplication.Music);
			}
			
			try
			{
				_cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken, new CancellationToken());
				await ffmpegProcess.StandardOutput.BaseStream.CopyToAsync(audioStream, 81920, _cancellationTokenSource.Token);
			}
			finally
			{
				await audioStream.FlushAsync();
			}	
		}
	}

	public void Stop()
	{
		try
		{
			Debug.Log("STOP");
			_cancellationTokenSource?.Cancel();
			if (audioStream != null)
			{
				//audioStream.ClearAsync(CancellationToken.None);
			}

			if (ffmpegProcess != null && !ffmpegProcess.HasExited)
			{
				ffmpegProcess.Kill();
				ffmpegProcess.Dispose();
			}
		}
		finally
		{
			ffmpegProcess = null;
			ToggleSpeaking(false);
		}
	}
	
	private Process CreateFFmpeg(string path)
	{
		//TODO: Expose Application.dataPath globally
		return Process.Start(new ProcessStartInfo
		{
			FileName = "E:/Dev/JeopardyUnity/Assets/ffmpeg/ffmpeg.exe",
			Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
			UseShellExecute = false,
			RedirectStandardOutput = true,
			CreateNoWindow = true
		});
	}
}
