using System.Collections;
using System.Collections.Generic;
using Discord;
using UnityEngine;

public class Gateway : AbstractDiscordApi {
    
    public Gateway(DiscordClient client) : base(client)
    {
    }

    protected override string Path()
    {
        return "gateway";
    }

    public string GetBotGateway()
    {
        return GET("bot");
    }
}
