using System;
using System.Net;

namespace Discord
{
    public abstract class DiscordClient : IDisposable
    {
        
        protected DiscordWebSocketClient webSocketClient;
        
        public abstract HttpWebRequest AddAuthorization(HttpWebRequest request);

        public Channel Channel(string id)
        {
            return new Channel(this, id);
        }

        public Gateway Gateway()
        {
            return new Gateway(this);
        }

        public void Dispose()
        {
            webSocketClient.Dispose();            
        }
    }
}


