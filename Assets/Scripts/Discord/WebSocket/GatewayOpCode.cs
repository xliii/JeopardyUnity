public enum GatewayOpCode
{
    Dispatch = 0,
    Heartbeat = 1,
    Identify = 2,
    StatusUpdate = 3,
    VoiceStateUpdate = 4,
    VoiceServerPing = 5,
    Resume = 6,
    Reconnect = 7,
    RequestGuildMembers = 8,
    InvalidSession = 9,
    Hello = 10,
    HeartbeatACK = 11,
    
    Voice_Identify = 0,
    Voice_SelectProtocol = 1,
    Voice_Ready = 2,
    Voice_Heartbeat = 3,
    Voice_SessionDescription = 4,
    Voice_Speaking = 5,
    Voice_HeartbeatACK = 6,
    Voice_Resume = 7,
    Voice_Hello = 8,
    Voice_Resumed = 9,
    Voice_ClientDisconnect = 13
}