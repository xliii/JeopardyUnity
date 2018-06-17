using System;
using System.Net;
using System.Net.Sockets;
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
        DiscoverIP();
    }

    private void DiscoverIP()
    {
        Debug.Log("Discover IP:");    
        byte[] packet = new byte[70];
        packet[0] = (byte)(ssrc >> 24);
        packet[1] = (byte)(ssrc >> 16);
        packet[2] = (byte)(ssrc >> 8);
        packet[3] = (byte)(ssrc >> 0);

        Send(packet);
    }

    public void Send(byte[] packet)
    {
        try
        {  
            _client.Send(packet, packet.Length);
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
                byte[] packet = _client.Receive(ref _endpoint);             
                logUDP(packet);
                Messenger.Broadcast(DiscordEvent.Voice.Packet, packet);
            }
            catch (ThreadAbortException)
            {
                Debug.Log("UDP Receiver closed");
                break;
            }
            catch (Exception e)
            {
                Debug.LogError($"UDP Receive Exception: {e}");
            }
        }
    }

    private void logUDP(byte[] packet)
    {
        Debug.Log($"UDP < {string.Join(", ", packet)}");
    }

    public void Dispose()
    {
        _receiveThread?.Abort();
        _client.Close();        
    }
}
