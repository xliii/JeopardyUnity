using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class HeartbeatService
{
    private DiscordWebSocketClient client;
    private int interval;

    private int? sequenceNumber;

    public HeartbeatService(DiscordWebSocketClient client, int interval)
    {
        this.client = client;
        this.interval = interval;
        Messenger.AddListener(GatewayOpCode.HeartbeatACK.Name(), OnHeartbeatAck);
        SendHeartbeat();
    }

    private void OnHeartbeatAck()
    {
        Debug.Log("Heartbeat ACK");
    }

    private void SendHeartbeat()
    {
        var heartbeat = new GatewayPayload
        {
            OpCode = GatewayOpCode.Heartbeat,
            Data = sequenceNumber			
        };
        
        client.Send(JsonConvert.SerializeObject(heartbeat));
    }
}
