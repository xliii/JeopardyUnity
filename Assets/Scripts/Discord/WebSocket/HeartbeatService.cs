public class HeartbeatService : AbstractHeartbeatService
{
    private int? sequenceNumber;

    public HeartbeatService(AbstractGatewayClient gateway, int interval) : base(gateway, interval)
    {
        Messenger.AddListener<int?>(DiscordEvent.SequenceNumber, OnSequenceNumberUpdated);
        Messenger.AddListener(DiscordEvent.HeartbeatACK, Acknowledge);
    }

    private void OnSequenceNumberUpdated(int? s)
    {       
        //Debug.Log($"Sequence number updated: {sequenceNumber}");
        sequenceNumber = s;
    }

    protected override GatewayOpCode OpCode => GatewayOpCode.Heartbeat;
    protected override object Data => sequenceNumber;
}
