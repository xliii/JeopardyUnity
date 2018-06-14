public class MessageCreateEventData : IGatewayEventData
{
    public const string Name = "MESSAGE_CREATE";

    public int type;
    public bool tts;
    public string timestamp; //TODO: timestamp
    public bool pinned;
    public string nonce;
    //TODO: Mentions
    public Member member;
    public string id;
    //TODO: Embeds
    //TODO: Edited timestamp
    public string content;
    public string channel_id;
    public User author;
    //TODO: Attachments
    public string guild_id;

}
