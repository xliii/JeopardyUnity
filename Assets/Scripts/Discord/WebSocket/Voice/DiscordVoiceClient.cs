using System;
using System.Text;
using UnityEngine;

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

	public VoiceUdpClient udpClient { get; private set; }

	public event EventHandler<SessionDesciptionResponse> OnVoiceReady;

	public DiscordVoiceClient(string userId, DiscordGatewayClient gateway)
	{
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

	public void SendVoice(AudioClip audioClip)
	{
		udpClient.SendVoice(audioClip);
	}
}
