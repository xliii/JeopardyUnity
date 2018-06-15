public class SpeakingResponse : IGatewayEventData
{
    public string user_id;
    public int ssrc;
    public bool speaking;
}