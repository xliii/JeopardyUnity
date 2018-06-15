public class DiscordVoiceGatewayClient : AbstractGatewayClient
{
	public DiscordVoiceGatewayClient(string gateway) : base(gateway) {}
	
	public override string Name => "Voice Gateway";

	protected override void OnMessage(GatewayPayload payload)
	{
		switch (payload.OpCode)
		{
			case GatewayOpCode.Voice_Hello:
				var helloData = Convert<HelloEventData>(payload.Data);									
				Messenger.Broadcast(DiscordEvent.Voice.Hello, helloData);					
				break;
			case GatewayOpCode.Voice_Ready:
				var readyData = Convert<VoiceReadyResponse>(payload.Data);
				Messenger.Broadcast(DiscordEvent.Voice.Ready, readyData);
				break;
			case GatewayOpCode.Voice_Heartbeat:
				var heartbeatData = Convert<int>(payload.Data);
				Messenger.Broadcast(DiscordEvent.Voice.HeartbeatACK, heartbeatData);
				break;
			case GatewayOpCode.Voice_SessionDescription:
				var sessionData = Convert<SessionDesciptionResponse>(payload.Data);
				Messenger.Broadcast(DiscordEvent.Voice.SessionDesciption, sessionData);
				break;
			case GatewayOpCode.Voice_Speaking:
				var speakingData = Convert<SpeakingResponse>(payload.Data);
				Messenger.Broadcast(DiscordEvent.Voice.Speaking, speakingData);
				break;
		}
		
	}
}
