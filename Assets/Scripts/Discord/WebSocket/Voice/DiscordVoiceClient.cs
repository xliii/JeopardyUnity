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

	private HeartbeatService heartbeatService;

	public DiscordVoiceClient(string userId, DiscordGatewayClient gateway)
	{
		this.gateway = gateway;
		this.userId = userId;
		
		//Voice initialization
		Messenger.AddListener<VoiceServerUpdate>(DiscordEvent.Voice.ServerUpdate, OnVoiceServerUpdate);
		Messenger.AddListener<VoiceStateUpdateResponse>(DiscordEvent.Voice.StatusUpdate, OnVoiceStatusUpdate);
		
		Messenger.AddListener<HelloEventData>(DiscordEvent.Voice.Hello, OnHello);
	}

	private void OnHello(HelloEventData e)
	{
		heartbeatService = new HeartbeatService(voiceGateway, e.heartbeat_interval);
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
	
	
	private void OnVoiceStatusUpdate(VoiceStateUpdateResponse e)
	{
		sessionId = e.session_id;
		TryConnect();
	}

	private void OnVoiceServerUpdate(VoiceServerUpdate e)
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
	}
}
