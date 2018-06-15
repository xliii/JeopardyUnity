using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class VoiceUdpClient : IDisposable
{
    public readonly string Mode = "xsalsa20_poly1305";
    private const int Timeout = 10000;

    private readonly Thread _receiveThread;
    private IPEndPoint _endpoint;
    private readonly UdpClient _client;

    public int ssrc { get; }

    public readonly int LocalPort = 1337;
    public byte[] SecretKey;

    public VoiceUdpClient(string hostname, int port, int ssrc)
    {
        _receiveThread = new Thread(Receive) {IsBackground = true};
        _endpoint = new IPEndPoint(IPAddress.Parse(hostname), port);
        _client = new UdpClient(LocalPort);
        this.ssrc = ssrc;
    }

    //TODO: Receiving voice
    //TODO: Sending voice 

    /// <summary>
    /// <a href="https://discordapp.com/developers/docs/topics/voice-connections#encrypting-and-sending-voice">Encrypting and sending voice</a>
    /// <para/>
    /// <a href="https://github.com/DevJohnC/Opus.NET">Opus Codec</a>
    /// <para/>
    /// <a href="https://github.com/adamcaudill/libsodium-net">Libsodium .NET</a>
    /// <para/>
    /// <a href="https://github.com/tabrath/libsodium-core">Libdodium .NET Standard 2.0</a>
    /// </summary>
    public void SendVoice()
    {
        
    }

    public void Start()
    {
        _client.Connect(_endpoint);
        //_client.Client.ReceiveTimeout = Timeout;
        //_client.Client.SendTimeout = Timeout;
        
        _receiveThread.Start();
        //DiscoverIP();
    }

    private void DiscoverIP()
    {
        Debug.Log("Discover IP:");
        byte[] header = BitConverter.GetBytes(ssrc);
        byte[] data = new byte[74];
        Array.Copy(header, data, header.Length);
        Send(data);
    }

    private void Send(byte[] data)
    {
        try
        {  
            _client.Send(data, data.Length);
        }
        catch (Exception e)
        {
            Debug.LogError($"UDP send exception: {e}");
        }
       
    }

    private void Receive()
    {
        while (true)
        {
            try
            {
                byte[] data = _client.Receive(ref _endpoint);
                
                string text = Encoding.UTF8.GetString(data);
                Debug.Log($"UDP < {text}");
            }
            catch (ThreadAbortException)
            {
                Debug.Log("UDP Receiver closed");
            }
            catch (Exception e)
            {
                Debug.LogError($"UDP Receive Exception: {e}");
            }
        }
    }

    public void Dispose()
    {
        _receiveThread?.Abort();
        _client.Close();        
    }
}
