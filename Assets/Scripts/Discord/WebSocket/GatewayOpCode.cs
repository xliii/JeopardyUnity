using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    HeartbeatACK = 11
}

public static class Extensions
{
    public static string Name(this GatewayOpCode opCode)
    {
        return opCode.ToString();
    }
}