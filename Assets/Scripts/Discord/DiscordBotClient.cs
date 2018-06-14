using System.Net;

namespace Discord
{
    public class DiscordBotClient : DiscordClient
    {
        private DiscordBotConfig config;

        public DiscordBotClient(DiscordBotConfig config) {
            this.config = config;
            Init();
        }

        public override HttpWebRequest AddAuthorization(HttpWebRequest request)
        {
            request.Headers.Add("Authorization", $"Bot {config.token}");
            return request;
        }

        protected override string GatewayUrl => Gateway().GetBotGateway().url;
    }

}