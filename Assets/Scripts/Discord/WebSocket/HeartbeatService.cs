using System;
using System.Timers;
using UnityEngine;

public class HeartbeatService : IDisposable
{
    private DiscordGatewayClient client;    
    private Timer timer;
    private bool acknowledged = true;

    private int? sequenceNumber;

    public HeartbeatService(DiscordGatewayClient client, int interval)
    {
        this.client = client;
        Messenger.AddListener(DiscordEvent.HeartbeatACK, OnHeartbeatAck); 
        Messenger.AddListener<int?>(DiscordEvent.SequenceNumber, OnSequenceNumberUpdated);
        
        timer = new Timer(interval);
        timer.Elapsed += (sender, args) => SendHeartbeat();
        timer.Start();         
        
        SendHeartbeat();
    }

    private void OnHeartbeatAck()
    {
        Debug.Log("Heartbeat ACK");
        acknowledged = true;
    }

    private void OnSequenceNumberUpdated(int? sequenceNumber)
    {       
        //Debug.Log($"Sequence number updated: {sequenceNumber}");
        this.sequenceNumber = sequenceNumber;
    }

    private void SendHeartbeat()
    {
        Debug.Log("Heartbeat");
        if (!acknowledged)
        {
            Debug.LogError("Previous heartbeat wasn't acknowledged");
        }
        var heartbeat = new GatewayPayload
        {
            OpCode = GatewayOpCode.Heartbeat,
            Data = sequenceNumber			
        };

        acknowledged = false;
        client.Send(heartbeat);
    }

    public void Dispose()
    {
        timer.Stop();        
    }
}
