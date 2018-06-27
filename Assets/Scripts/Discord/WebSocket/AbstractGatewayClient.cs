using System;
using Newtonsoft.Json;
using UnityEngine;
using WebSocketSharp;

public abstract class AbstractGatewayClient : IDisposable
{
    private WebSocket ws;
    
    public abstract string Name { get; }

    protected AbstractGatewayClient(string gateway)
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
        Debug.Log($"{Name} Connected");	
    }

    void OnMessage(object sender, MessageEventArgs e)
    {
        try
        {
            Debug.Log($"{Name} < {e.Data}");
            var payload = JsonConvert.DeserializeObject<GatewayPayload>(e.Data);
            OnMessage(payload);
        }
        catch (Exception exception)
        {
            Debug.LogError($"{Name} Receive Exception: \n  Data: {e.Data} \n\n  Exception: {exception} ");
        }
    }
    
    protected T Convert<T>(object obj)
    {
        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));
    }

    protected abstract void OnMessage(GatewayPayload payload);

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
        Debug.Log($"{Name} > {json}");
        ws.SendAsync(json, callback);
    }

    void OnSent(bool success)
    {
        if (!success)
        {
            Debug.LogError($"{Name} message failed");
        }
    }
    
    void OnClose(object sender, CloseEventArgs e)
    {
        Debug.Log($"{Name} Closed. Code: {e.Code}. Reason: {e.Reason}");
    }

    void OnError(object sender, ErrorEventArgs e)
    {
        Debug.Log($"{Name} Error. Message: {e.Message}. Exception: {e.Exception}");
    }

    public void Dispose()
    {
        ws?.CloseAsync();
    }
}