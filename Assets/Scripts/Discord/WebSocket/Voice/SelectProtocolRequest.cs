public class SelectProtocolRequest
{
    public string protocol;
    public ProtocolData data;
}

public class ProtocolData
{
    public string address;
    public int port;
    public string mode;
}