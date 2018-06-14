using System.IO;
using System.Net;
using System.Text;
using Discord;
using Newtonsoft.Json;
using UnityEngine;

public abstract class AbstractDiscordApi {
    
    private DiscordClient client;

    private readonly Encoding encoding = Encoding.UTF8;

    protected AbstractDiscordApi(DiscordClient client)
    {
        this.client = client;
    }

    private string API = "https://discordapp.com/api";

    protected T POST<T>(string path, object payload)
    {
        var fullPath = $"{API}/{Path()}/{path}";
        HttpWebRequest request = WebRequest.CreateHttp(fullPath);
        request.Method = "POST";
        request.ContentType = "application/json";
        client.AddAuthorization(request);
        
        AddPayload(request, payload);

        var response = GetResponse(request.GetResponse());
        return JsonConvert.DeserializeObject<T>(response);
    }

    protected T GET<T>(string path)
    {
        var fullPath = $"{API}/{Path()}/{path}";
        HttpWebRequest request = WebRequest.CreateHttp(fullPath);
        request.Method = "GET";
        request.ContentType = "application/json";
        client.AddAuthorization(request);

        var response = GetResponse(request.GetResponse());
        return JsonConvert.DeserializeObject<T>(response);
    }

    private void AddPayload(HttpWebRequest request, object payload)
    {
        var json = JsonUtility.ToJson(payload);
        var bytes = encoding.GetBytes(json);
        request.ContentLength = bytes.Length;

        var stream = request.GetRequestStream();
        stream.Write(bytes, 0, bytes.Length);
        stream.Close();
    }

    private string GetResponse(WebResponse response)
    {
        using (var responseStream = response.GetResponseStream())
        {
            using (var reader = new StreamReader(responseStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
    
    protected abstract string Path();
}
