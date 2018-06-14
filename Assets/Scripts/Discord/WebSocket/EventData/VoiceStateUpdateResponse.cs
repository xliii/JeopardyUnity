public class VoiceStateUpdateResponse : IGatewayEventData
{
    public const string Name = "VOICE_STATE_UPDATE";

    public Member member;
    public string user_id;
    public bool suppress;
    public string session_id;
    public bool self_video;
    public bool self_mute;
    public bool self_deaf;
    public bool mute;
    public bool deaf;
    public string guild_id;
    public string channel_id;
}
