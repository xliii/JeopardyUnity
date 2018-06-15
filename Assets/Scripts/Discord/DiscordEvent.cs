public static class DiscordEvent
{

    public const string Hello = "Discord.Hello";
    public const string HeartbeatACK = "Discord.HeartbeatACK";
    public const string SequenceNumber = "Discord.SequenceNumber";
    public const string TypingStart = "Discord.StartTyping";
    public const string MessageCreate = "Discord.MessageCreate";
    public const string GuildCreate = "Discord.GuildCreate";
    public const string Ready = "Discord.Ready";

    public static class Voice
    {
        public const string ServerUpdate = "Discord.Voice.ServerUpdate";
        public const string StatusUpdate = "Discord.Voice.StatusUpdate";
        public const string Hello = "Discord.Voice.Hello";
        public const string Ready = "Discord.Voice.Ready";
        public const string HeartbeatACK = "Discord.Voice.HeartbeatACK";
    }
}
