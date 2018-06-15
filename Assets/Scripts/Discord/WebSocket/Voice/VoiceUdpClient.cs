using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class VoiceUdpClient : IDisposable
{
    public const string Mode = "xsalsa20_poly1305";
    private const int Timeout = 10000;   

    private IPEndPoint endpoint;

    private Thread receiveThread;

    private UdpClient _client;

    public VoiceUdpClient(string hostname, int port)
    {
        endpoint = new IPEndPoint(IPAddress.Parse(hostname), port);
        _client = new UdpClient();
    }

    public void StartReceiving()
    {
        _client.Connect(endpoint);
        //_client.Client.ReceiveTimeout = Timeout;
        //_client.Client.SendTimeout = Timeout;
        receiveThread = new Thread(Receive) {IsBackground = true};
        receiveThread.Start();
    }

    private void Receive()
    {
        while (true)
        {
            try
            {
                byte[] data = _client.Receive(ref endpoint);
                string text = Encoding.UTF8.GetString(data);
                Debug.Log($"UDP < {text}");
            }
            catch (SocketException e)
            {
                if (e.ErrorCode == 10060)
                {
                    Debug.LogError("UDP Receive timeout");
                }
                else
                {
                    Debug.LogError($"UDP Socket exception: {e}");
                }
                
            }
            catch (Exception e)
            {
                Debug.LogError($"UDP Exception: {e}");
            }
        }
    }

    public void Dispose()
    {
        receiveThread?.Abort();
        _client.Close();        
    }
}
