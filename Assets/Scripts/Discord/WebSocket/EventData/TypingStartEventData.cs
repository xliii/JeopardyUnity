public class TypingStartEventData : IGatewayEventData
{
    public const string Name = "TYPING_START";

    public string user_id;
    public long timestamp;
    public Member member;
    public string channel_id;
    public string guild_id;

}

public class Member
{
    public User user;
    public bool mute;
    public string joined_at; //TODO: use timestamp;
    public bool deaf;
    //TODO: Roles
}

public class User
{
    public bool? verified;
    public string username;
    public bool? mfa_enabled;
    public string id;
    public string email;
    public string discriminator;
    public bool? bot;
    public string avatar;
}
