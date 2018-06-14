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

    public GetBotGatewayResponse GetBotGateway()
    {
        var response = GET("bot");
        return JsonUtility.FromJson<GetBotGatewayResponse>(response);
    }   
}

public class GetBotGatewayResponse
{
    public string url;
    public int shards;
}