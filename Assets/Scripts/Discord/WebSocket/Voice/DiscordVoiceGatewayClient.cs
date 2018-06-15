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
			/*case GatewayOpCode.HeartbeatACK:
				Messenger.Broadcast(DiscordEvent.HeartbeatACK);
				break;
			case GatewayOpCode.Dispatch:
				Messenger.Broadcast(DiscordEvent.SequenceNumber, payload.SequenceNumber);
				switch (payload.EventName)
				{
					case TypingStartEventData.Name:
						var typingData = Convert<TypingStartEventData>(payload.Data);
						Messenger.Broadcast(DiscordEvent.TypingStart, typingData);
						break;
					case MessageCreateEventData.Name:
						var messageData = Convert<MessageCreateEventData>(payload.Data);
						Messenger.Broadcast(DiscordEvent.MessageCreate, messageData);							
						break;
					case ReadyEventData.Name:
						var readyData = Convert<ReadyEventData>(payload.Data);
						Messenger.Broadcast(DiscordEvent.Ready, readyData);							
						break;
					case GuildCreateEventData.Name:
						var guildData = Convert<GuildCreateEventData>(payload.Data);
						Messenger.Broadcast(DiscordEvent.GuildCreate, guildData);						
						break;
					case VoiceServerUpdate.Name:
						var voiceServerUpdate = Convert<VoiceServerUpdate>(payload.Data);
						Messenger.Broadcast(DiscordEvent.Voice.ServerUpdate, voiceServerUpdate);
						break;
					case VoiceStateUpdateResponse.Name:
						var voiceStateUpdate = Convert<VoiceStateUpdateResponse>(payload.Data);
						Messenger.Broadcast(DiscordEvent.Voice.StatusUpdate, voiceStateUpdate);
						break;
				}
				break;		*/		 
		}
		
	}
}
