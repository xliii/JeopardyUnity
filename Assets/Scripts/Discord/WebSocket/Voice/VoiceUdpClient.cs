using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class VoiceUdpClient : IDisposable
{
    public readonly string Mode = "xsalsa20_poly1305";
    private const int Timeout = 10000;

    private readonly Thread _receiveThread;
    private IPEndPoint _endpoint;
    private readonly UdpClient _client;

    public uint ssrc { get; }

    public readonly int LocalPort = 1338;
    public byte[] SecretKey;

    public VoiceUdpClient(string hostname, int port, uint ssrc)
    {
        _receiveThread = new Thread(Receive) {IsBackground = true};
        _endpoint = new IPEndPoint(IPAddress.Parse(hostname), port);
        _client = new UdpClient(LocalPort);
        this.ssrc = ssrc;
    }

    public void SendVoice(AudioClip clip)
    {
        Debug.Log($"Send Voice: {clip.name} Samples: {clip.samples}");         
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
        Send(packet, 0, packet.Length);
    }

    public async Task<int> SendAsync(byte[] packet)
    {
        return await SendAsync(packet, 0, packet.Length);
    }

    public async Task<int> SendAsync(byte[] packet, int offset, int bytes)
    {
        try
        {
            //logOutgoing(packet);
            return await _client.SendAsync(packet, bytes);
        }
        catch (Exception e)
        {
            Debug.LogError($"UDP send exception: {e}");
            return 0;
        }
    }

    public void Send(byte[] packet, int offset, int bytes)
    {
        try
        {
            //logOutgoing(packet);
            _client.Send(packet, bytes);
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
                //logIncoming(packet);
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

    private void logIncoming(byte[] packet)
    {
        log(packet, "<<");
    }

    private void logOutgoing(byte[] packet)
    {
        log(packet, ">>");
    }

    private void log(byte[] packet, string type)
    {
        Debug.Log($"UDP {type} ({packet.Length}) {string.Join(", ", packet)}");
    }

    public void Dispose()
    {
        _receiveThread?.Abort();
        _client.Close();        
    }
}
