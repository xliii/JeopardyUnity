using System;
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

	private VoiceUdpClient udpClient;

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
	}

	private void OnSessionDescription(SessionDesciptionResponse e)
	{
		udpClient.SecretKey = e.secret_key;
		ToggleSpeaking(true);
	}

	private void ToggleSpeaking(bool speaking)
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
		//Select protocol
		var payload = new GatewayPayload
		{
			OpCode = GatewayOpCode.Voice_SelectProtocol,
			Data = new SelectProtocolRequest
			{
				protocol = "udp",
				data = new ProtocolData
				{
					address = "127.0.0.1",
					port = udpClient.LocalPort,
					mode = udpClient.Mode
				}
			}
		};
		voiceGateway.Send(payload);
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
			Debug.LogWarning("Not enough data to establish voice connection");
			return;
		}
		
		voiceGateway = new DiscordVoiceGatewayClient(endpoint);
	}

	public void Dispose()
	{
		voiceGateway.Dispose();
		heartbeatService.Dispose();
		udpClient.Dispose();
	}
}
