using System;
using System.Net;
using Newtonsoft.Json;
using UnityEngine;

namespace Discord
{
    public abstract class DiscordClient : IDisposable
    {
        private HeartbeatService heartbeatService;
        
        protected DiscordWebSocketClient webSocketClient;
        
        protected void Init()
        {
            webSocketClient = new DiscordWebSocketClient(GatewayUrl);
            Messenger.AddListener<HelloEventData>(DiscordEvent.Hello, OnHello);
            Messenger.AddListener(DiscordEvent.HeartbeatACK, OnInitialHeartbeatACK);
        }

        private void OnInitialHeartbeatACK()
        {
            Messenger.RemoveListener(DiscordEvent.HeartbeatACK, OnInitialHeartbeatACK);
            Debug.Log("Initial ACK");
            
            //Identify
            var identify = new GatewayPayload
            {
                OpCode = GatewayOpCode.Identify,
                Data = new IdentifyEventData
                {
                    token = Token,
                    properties = new ConnectionProperties
                    {
                        os = "windows",
                        browser = "xliii",
                        device = "xliii"
                    }
                }
            };
            
            var payload = JsonConvert.SerializeObject(identify, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            
            webSocketClient.Send(payload);
        }
        
        protected abstract string Token { get; }

        protected abstract string GatewayUrl { get; }
        
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


