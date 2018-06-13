using System;
using UnityEngine;
using WebSocketSharp;

public class DiscordWebSocketClient : IDisposable {
	
	private WebSocket ws;

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
		Debug.Log($"WebSocket Message Received: {e.Data}");
	}

	void OnClose(object sender, CloseEventArgs e)
	{
		Debug.Log($"WebSocket Closed: {e.Reason}");
	}

	void OnError(object sender, ErrorEventArgs e)
	{
		Debug.Log($"WebSocket Error: {e.Message}");
	}

	public void Dispose()
	{
		ws.CloseAsync();
	}
}
