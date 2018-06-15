using System;

public class VoiceHeartbeatService : AbstractHeartbeatService
{
	private Random random = new Random();

	private const double INTERVAL_MULTIPLIER = 0.75;

	public VoiceHeartbeatService(AbstractGatewayClient gateway, int interval) : base(gateway, (int) (interval * INTERVAL_MULTIPLIER))
	{
		Messenger.AddListener<int>(DiscordEvent.Voice.HeartbeatACK, OnHeartbeatAck); 
	}
	
	private void OnHeartbeatAck(int nonce)
	{
		Acknowledge(); //TODO: Check nonce
	}

	protected override GatewayOpCode OpCode => GatewayOpCode.Voice_Heartbeat;
	protected override object Data => random.Next();
}
