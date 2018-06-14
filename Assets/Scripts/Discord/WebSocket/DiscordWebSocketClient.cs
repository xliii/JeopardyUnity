using System;
using Newtonsoft.Json;
using UnityEngine;
using WebSocketSharp;

public class DiscordWebSocketClient : IDisposable {
	
	private WebSocket ws;

	private int heartbeatInterval;
	private int? sequenceNumber;

	public DiscordWebSocketClient(string gateway)
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
		Debug.Log("WebSocket Connected");	
	}

	void OnMessage(object sender, MessageEventArgs e)
	{
		try
		{
			Debug.Log($"WebSocket Message Received: {e.Data}");
			var payload = JsonConvert.DeserializeObject<GatewayPayload>(e.Data);			
			switch (payload.OpCode)
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
						{
							var typingData = Convert<TypingStartEventData>(payload.Data);
							Messenger.Broadcast(DiscordEvent.TypingStart, typingData);
							break;
						}
						case MessageCreateEventData.Name:
							var messageData = Convert<MessageCreateEventData>(payload.Data);
							Messenger.Broadcast(DiscordEvent.MessageCreate, messageData);
							Debug.Log($"{messageData.author.username}: {messageData.content}");
							break;
					}
					break;
			}
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
	
	public void Send(string payload)
	{
		Send(payload, OnSent);
	}

	public void Send(string payload, Action<bool> callback)
	{
		ws.SendAsync(payload, callback);
	}

	void OnSent(bool success)
	{
		if (!success)
		{
			Debug.LogError("Send message failed");
		}
	}

	void OnClose(object sender, CloseEventArgs e)
	{
		Debug.Log($"WebSocket Closed. Code: {e.Code}. Reason: {e.Reason}");
	}

	void OnError(object sender, ErrorEventArgs e)
	{
		Debug.Log($"WebSocket Error. Message: {e.Message}. Exception: {e.Exception}");
	}

	public void Dispose()
	{
		ws.CloseAsync();
	}
}
