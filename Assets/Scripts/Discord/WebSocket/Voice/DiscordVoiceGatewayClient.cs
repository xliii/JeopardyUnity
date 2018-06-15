using System;
using Newtonsoft.Json;
using UnityEngine;
using WebSocketSharp;

public class DiscordVoiceGatewayClient : IDisposable {
	
	private WebSocket ws;

	public DiscordVoiceGatewayClient(string gateway)
	{
		ws = new WebSocket(gateway);
		
		ws.OnOpen += OnOpen;
		ws.OnMessage += OnMessage;
		ws.OnClose += OnClose;
		ws.OnError += OnError;
		
		ws.ConnectAsync();
	}
	
	void OnOpen(object sender, EventArgs e)
	{
		Debug.Log("Voice WebSocket Connected");	
	}

	void OnMessage(object sender, MessageEventArgs e)
	{
		try
		{
			Debug.Log($"Voice WebSocket Message Received: {e.Data}");
			var payload = JsonConvert.DeserializeObject<GatewayPayload>(e.Data);			
			/*switch (payload.OpCode)
			{
				case GatewayOpCode.Hello:
					var helloData = Convert<HelloEventData>(payload.Data);									
					Messenger.Broadcast(DiscordEvent.Hello, helloData);					
					break;
				case GatewayOpCode.HeartbeatACK:
					Messenger.Broadcast(DiscordEvent.HeartbeatACK);
					break;
				case GatewayOpCode.Dispatch:
					Messenger.Broadcast(DiscordEvent.SequenceNumber, payload.SequenceNumber);
					switch (payload.EventName)
					{
						case TypingStartEventData.Name:
							var typingData = Convert<TypingStartEventData>(payload.Data);
							Messenger.Broadcast(DiscordEvent.TypingStart, typingData);
							break;
						case MessageCreateEventData.Name:
							var messageData = Convert<MessageCreateEventData>(payload.Data);
							Messenger.Broadcast(DiscordEvent.MessageCreate, messageData);							
							break;
						case ReadyEventData.Name:
							var readyData = Convert<ReadyEventData>(payload.Data);
							Messenger.Broadcast(DiscordEvent.Ready, readyData);							
							break;
						case GuildCreateEventData.Name:
							var guildData = Convert<GuildCreateEventData>(payload.Data);
							Messenger.Broadcast(DiscordEvent.GuildCreate, guildData);						
							break;
						case VoiceServerUpdate.Name:
							var voiceServerUpdate = Convert<VoiceServerUpdate>(payload.Data);
							Messenger.Broadcast(DiscordEvent.VoiceServerUpdate, voiceServerUpdate);
							break;
						case VoiceStateUpdateResponse.Name:
							var voiceStateUpdate = Convert<VoiceStateUpdateResponse>(payload.Data);
							Messenger.Broadcast(DiscordEvent.VoiceStatusUpdate, voiceStateUpdate);
							break;
					}
					break;				 
			}*/
		}
		catch (Exception exception)
		{
			Debug.LogError(exception.Message);
		}
		
	}

	T Convert<T>(object obj)
	{
		return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));
	}
	
	public void Send(object payload)
	{
		Send(payload, OnSent);
	}

	public void Send(object payload, Action<bool> callback)
	{
		var json = JsonConvert.SerializeObject(payload, new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore
		});
		ws.SendAsync(json, callback);
	}

	void OnSent(bool success)
	{
		if (!success)
		{
			Debug.LogError("Voice Send message failed");
		}
	}

	void OnClose(object sender, CloseEventArgs e)
	{
		Debug.Log($"Voice WebSocket Closed. Code: {e.Code}. Reason: {e.Reason}");
	}

	void OnError(object sender, ErrorEventArgs e)
	{
		Debug.Log($"Voice WebSocket Error. Message: {e.Message}. Exception: {e.Exception}");
	}

	public void Dispose()
	{
		ws.CloseAsync();
	}
}
