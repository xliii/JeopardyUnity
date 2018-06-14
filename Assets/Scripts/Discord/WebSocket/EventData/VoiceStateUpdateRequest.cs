public class VoiceStateUpdateRequest : IGatewayEventData
{

    public string guild_id;
    public string channel_id;
    public bool self_mute;
    public bool self_deaf;
}
