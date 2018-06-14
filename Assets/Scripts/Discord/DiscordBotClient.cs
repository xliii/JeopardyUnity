using System.Net;

namespace Discord
{
    public class DiscordBotClient : DiscordClient
    {
        private DiscordBotConfig config;

        public DiscordBotClient(DiscordBotConfig config) {
            this.config = config;
        }

        public override void AddAuthorization(HttpWebRequest request)
        {
            request.Headers.Add("Authorization", $"Bot {config.token}");            
        }

        protected override string GatewayUrl => Gateway().GetBotGateway().url;

        protected override string Token => config.token;
    }

}