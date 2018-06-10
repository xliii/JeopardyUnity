using System.IO;
using System.Net;
using System.Text;
using Discord;
using UnityEngine;

public abstract class AbstractDiscordApi {
    
    private DiscordClient client;

    private readonly Encoding encoding = Encoding.UTF8;

    protected AbstractDiscordApi(DiscordClient client)
    {
        this.client = client;
    }

    private string API = "https://discordapp.com/api";

    protected HttpWebResponse POST(string path, object payload)
    {
        var fullPath = $"{API}/{Path()}/{path}";
        HttpWebRequest request = WebRequest.CreateHttp(fullPath);
        request.Method = "POST";
        request.ContentType = "application/json";
        client.AddAuthorization(request);
        
        AddPayload(request, payload);

        return request.GetResponse() as HttpWebResponse;       
    }

    private HttpWebRequest AddPayload(HttpWebRequest request, object payload)
    {
        var json = JsonUtility.ToJson(payload);
        var bytes = encoding.GetBytes(json);
        request.ContentLength = bytes.Length;

        var stream = request.GetRequestStream();
        stream.Write(bytes, 0, bytes.Length);
        stream.Close();

        return request;
    }

    private string GetResponse(HttpWebResponse response)
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
