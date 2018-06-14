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
					var helloEventData = Convert<HelloEventData>(payload.Data);									
					Messenger.Broadcast(GatewayOpCode.Hello.Name(), helloEventData);					
					break;
				case GatewayOpCode.HeartbeatACK:
					Messenger.Broadcast(GatewayOpCode.HeartbeatACK.Name());
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
		Debug.Log(success ? "Message sent" : "Message failed");
	}

	void OnClose(object sender, CloseEventArgs e)
	{
		Debug.Log($"WebSocket Closed: {e.Reason}");
	}

	void OnError(object sender, ErrorEventArgs e)
	{
		Debug.Log($"WebSocket Error: {e.Message} - {e.Exception}");
	}

	public void Dispose()
	{
		ws.CloseAsync();
	}
}
