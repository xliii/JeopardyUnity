using System;
using System.Net;
using UnityEngine;

namespace Discord
{
    public abstract class DiscordClient : IDisposable
    {
        private HeartbeatService heartbeatService;
        
        private DiscordGatewayClient gateway;

        private DiscordVoiceClient voiceClient;

        private string userId;

        public event EventHandler<ReadyEventData> OnReady;

        public event EventHandler<MessageCreateEventData> OnMessage;

        protected DiscordClient()
        {            
            //Initialization
            Messenger.AddListener<HelloEventData>(DiscordEvent.Hello, OnHello);
            Messenger.AddListener(DiscordEvent.HeartbeatACK, OnInitialHeartbeatACK);
            OnReady += OnReadyUser;
            
            //Public
            Messenger.AddListener<ReadyEventData>(DiscordEvent.Ready, e => OnReady.Invoke(this, e));
            Messenger.AddListener<MessageCreateEventData>(DiscordEvent.MessageCreate, e => OnMessage.Invoke(this, e));
        }

        private void OnReadyUser(object sender, ReadyEventData e)
        {
            userId = e.user.id;
        }

        public void Connect()
        {
            gateway = new DiscordGatewayClient(GatewayUrl);            
        }

        public void JoinVoice(string guildId, string channelId)
        {            
            if (userId == null) throw new Exception("Client not ready");
            voiceClient = new DiscordVoiceClient(userId, gateway);
            voiceClient.JoinVoice(guildId, channelId);
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
            
            gateway.Send(payload);
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
            gateway.Dispose();
            heartbeatService.Dispose();
        }

        private void OnHello(HelloEventData e)
        {
            heartbeatService = new HeartbeatService(gateway, e.heartbeat_interval);
        }
    }
}


