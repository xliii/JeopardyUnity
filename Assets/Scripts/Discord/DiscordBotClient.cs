using System.Net;

namespace Discord
{
    public class DiscordBotClient : DiscordClient
    {
        private DiscordBotConfig config;

        public DiscordBotClient(DiscordBotConfig config) {
            this.config = config;
            webSocketClient = new DiscordWebSocketClient(Gateway().GetBotGateway().url);
        }

        public override HttpWebRequest AddAuthorization(HttpWebRequest request)
        {
            request.Headers.Add("Authorization", $"Bot {config.token}");
            return request;
        }
    }

}