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

    public GatewayResponse GetBotGateway()
    {
        var response = GET("bot");
        return JsonUtility.FromJson<GatewayResponse>(response);
    }   
}

public class GatewayResponse
{
    public string url;
    public int shards;
}