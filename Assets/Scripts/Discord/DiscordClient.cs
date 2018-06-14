using System;
using System.Net;
using UnityEngine;

namespace Discord
{
    public abstract class DiscordClient : IDisposable
    {
        private HeartbeatService heartbeatService;
        
        protected void Init()
        {
            webSocketClient = new DiscordWebSocketClient(GatewayUrl);
            Messenger.AddListener<HelloEventData>(GatewayOpCode.Hello.Name(), OnHello);
        }
        
        protected abstract string GatewayUrl { get; }

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

        private void OnHello(HelloEventData e)
        {
            heartbeatService = new HeartbeatService(webSocketClient, e.heartbeat_interval);
        }
    }
}


