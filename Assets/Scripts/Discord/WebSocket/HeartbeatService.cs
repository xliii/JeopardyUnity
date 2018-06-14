using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Newtonsoft.Json;
using UnityEngine;

public class HeartbeatService
{
    private DiscordWebSocketClient client;
    private int interval;
    private Timer timer;
    private bool acknowledged = true;

    private int? sequenceNumber;

    public HeartbeatService(DiscordWebSocketClient client, int interval)
    {
        this.client = client;
        this.interval = interval;
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
        client.Send(JsonConvert.SerializeObject(heartbeat));
    }
}
