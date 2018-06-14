using System;
using System.Net;
using Newtonsoft.Json;
using UnityEngine;

namespace Discord
{
    public abstract class DiscordClient : IDisposable
    {
        private HeartbeatService heartbeatService;
        
        private DiscordWebSocketClient webSocketClient;

        public event EventHandler<ReadyEventData> OnReady;

        public event EventHandler<MessageCreateEventData> OnMessage;

        protected DiscordClient()
        {
            Messenger.AddListener<HelloEventData>(DiscordEvent.Hello, OnHello);
            Messenger.AddListener(DiscordEvent.HeartbeatACK, OnInitialHeartbeatACK);
            Messenger.AddListener<ReadyEventData>(DiscordEvent.Ready, e => OnReady.Invoke(this, e));
            Messenger.AddListener<MessageCreateEventData>(DiscordEvent.MessageCreate, e => OnMessage.Invoke(this, e));
        }

        public void Connect()
        {             
            webSocketClient = new DiscordWebSocketClient(GatewayUrl);
        }

        public void JoinVoice(string guildId, string channelId)
        {
            var payload = new GatewayPayload
            {
                OpCode = GatewayOpCode.VoiceStateUpdate,
                Data = new VoiceStateUpdateData
                {
                    guild_id = guildId,
                    channel_id = channelId,
                    self_mute = false,
                    self_deaf = false
                }
            };
            
            webSocketClient.Send(payload);
        }

        private void OnInitialHeartbeatACK()
        {
            Messenger.RemoveListener(DiscordEvent.HeartbeatACK, OnInitialHeartbeatACK);
            Debug.Log("Initial ACK received. Proceed to Identify");
            
            //Identify
            var payload = new GatewayPayload
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
            
            webSocketClient.Send(payload);
        }
        
        protected abstract string Token { get; }

        protected abstract string GatewayUrl { get; }
        
        public abstract void AddAuthorization(HttpWebRequest request);

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
            heartbeatService.Dispose();
        }

        private void OnHello(HelloEventData e)
        {
            heartbeatService = new HeartbeatService(webSocketClient, e.heartbeat_interval);
        }
    }
}


