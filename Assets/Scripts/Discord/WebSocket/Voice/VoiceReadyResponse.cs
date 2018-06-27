using System.Collections.Generic;

public class VoiceReadyResponse : IGatewayEventData
{
    public uint ssrc;
    public string ip;
    public int port;
    public List<string> modes;
    public int heartbeat_interval; //IGNORE    
}