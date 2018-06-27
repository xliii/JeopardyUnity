using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using unity.libsodium;
using UnityEngine;
using UnityParseHelpers;

namespace Discord
{
    public abstract class DiscordClient : IDisposable
    {
        private IHeartbeatService heartbeatService;
        
        private DiscordGatewayClient gateway;

        public DiscordVoiceClient voiceClient;

        private string userId;

        public event EventHandler<ReadyEventData> OnReady;

        public event EventHandler<MessageCreateEventData> OnMessage;

        public event EventHandler<SessionDesciptionResponse> OnVoiceReady;
        
        public const int MaxBitrate = 128 * 1024;
        
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        protected DiscordClient()
        {            
            //Initialization
            Messenger.AddListener<HelloEventData>(DiscordEvent.Hello, OnHello);
            Messenger.AddListener(DiscordEvent.HeartbeatACK, OnInitialHeartbeatACK);
            OnReady += OnReadyUser;
            
            //Public
            Messenger.AddListener<ReadyEventData>(DiscordEvent.Ready, e => OnReady.Invoke(this, e));
            Messenger.AddListener<MessageCreateEventData>(DiscordEvent.MessageCreate, e => OnMessage.Invoke(this, e));

            //Initialize Loom from main thread
            Loom.Instance.Clear();
        }

        private void OnReadyUser(object sender, ReadyEventData e)
        {
            userId = e.user.id;
        }

        public void Connect()
        {
            gateway = new DiscordGatewayClient(GatewayUrl);            
        }

        public void JoinVoice(string guildId, string channelId)
        {            
            if (userId == null) throw new Exception("Client not ready");
            voiceClient = new DiscordVoiceClient(userId, gateway);
            voiceClient.OnVoiceReady += OnVoiceReadyInternal;
            voiceClient.JoinVoice(guildId, channelId);
        }

        private void OnVoiceReadyInternal(object sender, SessionDesciptionResponse e)
        {
            Loom.Instance.QueueOnMainThread(() => OnVoiceReady.Invoke(sender, e));
        }

        private void OnInitialHeartbeatACK()
        {
            Messenger.RemoveListener(DiscordEvent.HeartbeatACK, OnInitialHeartbeatACK);
            Debug.Log("Initial ACK received. Proceed to Identify");
            
            //Identify
            var payload = new GatewayPayload
            {
                OpCode = GatewayOpCode.Identify,
                Data = new IdentifyEventData
                {
                    token = Token,
                    properties = new ConnectionProperties
                    {
                        os = "windows",
                        browser = "xliii",
                        device = "xliii"
                    }
                }
            };
            
            gateway.Send(payload);
        }
        
        protected abstract string Token { get; }

        protected abstract string GatewayUrl { get; }
        
        public abstract void AddAuthorization(HttpWebRequest request);

        public Channel Channel(string id)
        {
            return new Channel(this, id);
        }

        public Gateway Gateway()
        {
            return new Gateway(this);
        }

        public void Dispose()
        {
            gateway.Dispose();
            voiceClient.Dispose();
            heartbeatService.Dispose();
            _cancellationTokenSource.Cancel();
        }

        private void OnHello(HelloEventData e)
        {
            heartbeatService = new HeartbeatService(gateway, e.heartbeat_interval);
            heartbeatService.Start();
        }

        public void SendVoice(AudioClip audioClip)
        {
            voiceClient.SendVoice(audioClip);
        }
        
        public AudioOutStream CreatePCMStream(AudioApplication application, int? bitrate = null, int bufferMillis = 1000, int packetLoss = 30)        
        {
            var outputStream = new OutputStream(voiceClient.udpClient); //Ignores header
            var sodiumEncrypter = new SodiumEncryptStream(outputStream, voiceClient); //Passes header
            var rtpWriter = new RTPWriteStream(sodiumEncrypter, voiceClient.udpClient.ssrc); //Consumes header, passes
            var bufferedStream = new BufferedWriteStream(rtpWriter, voiceClient, bufferMillis, _cancellationTokenSource.Token); //Ignores header, generates header
            return new OpusEncodeStream(bufferedStream, bitrate ?? (96 * 1024), application, packetLoss); //Generates header
        }
        
        public AudioOutStream CreateDirectPCMStream(AudioApplication application, int? bitrate = null, int packetLoss = 30)
        {
            var outputStream = new OutputStream(voiceClient.udpClient); //Ignores header
            var sodiumEncrypter = new SodiumEncryptStream(outputStream, voiceClient); //Passes header
            var rtpWriter = new RTPWriteStream(sodiumEncrypter, voiceClient.udpClient.ssrc); //Consumes header, passes
            return new OpusEncodeStream(rtpWriter, bitrate ?? (96 * 1024), application, packetLoss); //Generates header
        }
    }

    public enum AudioApplication : int
    {
        Voice,
        Music,
        Mixed
    }
    
    public abstract class AudioStream : Stream
    {
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => false;

        public virtual void WriteHeader(ushort seq, uint timestamp, bool missed) 
        { 
            throw new InvalidOperationException("This stream does not accept headers");
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            //Debug.Log("AudioStream:Write");
            WriteAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();
        }
        public override void Flush()
        {
            FlushAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
        public void Clear()
        {
            ClearAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        public virtual Task ClearAsync(CancellationToken cancellationToken) { return Task.Delay(0); }

        public override long Length { get { throw new NotSupportedException(); } }
        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }
        public override void SetLength(long value) { throw new NotSupportedException(); }
        public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
    }
    
    public abstract class AudioOutStream : AudioStream
    {
        public override bool CanWrite => true;

        public override int Read(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }
        public override void SetLength(long value) { throw new NotSupportedException(); }
        public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
    }
    
    public class OutputStream : AudioOutStream
    {
        private readonly VoiceUdpClient _client;

        internal OutputStream(VoiceUdpClient client)
        {
            _client = client;
        }
        
        public override void WriteHeader(ushort seq, uint timestamp, bool missed) { } //Ignore
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancelToken)
        {
            //Debug.Log("OutputStream:WriteAsync");
            cancelToken.ThrowIfCancellationRequested();
            await _client.SendAsync(buffer, offset, count);
        }
    }
    
    public class SodiumEncryptStream : AudioOutStream
    {
        private readonly DiscordVoiceClient _client;
        private readonly AudioStream _next;
        private readonly byte[] _nonce;
        private bool _hasHeader;
        private ushort _nextSeq;
        private uint _nextTimestamp;

        public SodiumEncryptStream(AudioStream next, DiscordVoiceClient client)
        {
            _next = next;
            _client = client;
            _nonce = new byte[24];
        }
        
        public override void WriteHeader(ushort seq, uint timestamp, bool missed)
        {
            if (_hasHeader)
                throw new InvalidOperationException("Header received with no payload");

            _nextSeq = seq;
            _nextTimestamp = timestamp;
            _hasHeader = true;
        }
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancelToken)
        {
            //Debug.Log("SodiumEncryptStream:WriteAsync");
            cancelToken.ThrowIfCancellationRequested();
            if (!_hasHeader)
                throw new InvalidOperationException("Received payload without an RTP header");
            _hasHeader = false;

            if (SecretKey == null)
                return;
                
            Buffer.BlockCopy(buffer, offset, _nonce, 0, 12); //Copy nonce from RTP header
            count = SecretBox.Encrypt(buffer, offset + 12, count - 12, buffer, 12, _nonce, SecretKey);            
            _next.WriteHeader(_nextSeq, _nextTimestamp, false);
            await _next.WriteAsync(buffer, 0, count + 12, cancelToken).ConfigureAwait(false);
        }

        private byte[] SecretKey => _client.udpClient.SecretKey;

        public override async Task FlushAsync(CancellationToken cancelToken)
        {
            await _next.FlushAsync(cancelToken).ConfigureAwait(false);
        }
        public override async Task ClearAsync(CancellationToken cancelToken)
        {
            await _next.ClearAsync(cancelToken).ConfigureAwait(false);
        }
    }
    
    public class RTPWriteStream : AudioOutStream
    {
        private readonly AudioStream _next;
        private readonly byte[] _header;
        protected readonly byte[] _buffer;
        private uint _ssrc;
        private ushort _nextSeq;
        private uint _nextTimestamp;
        private bool _hasHeader;

        public RTPWriteStream(AudioStream next, uint ssrc, int bufferSize = 4000)
        {
            _next = next;
            _ssrc = ssrc;
            _buffer = new byte[bufferSize];
            _header = new byte[24];
            _header[0] = 0x80;
            _header[1] = 0x78;
            _header[8] = (byte)(_ssrc >> 24);
            _header[9] = (byte)(_ssrc >> 16);
            _header[10] = (byte)(_ssrc >> 8);
            _header[11] = (byte)(_ssrc >> 0);
        }

        public override void WriteHeader(ushort seq, uint timestamp, bool missed)
        {
            if (_hasHeader)
                throw new InvalidOperationException("Header received with no payload");
                
            _hasHeader = true;
            _nextSeq = seq;
            _nextTimestamp = timestamp;
        }
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancelToken)
        {
            //Debug.Log("RTPWriteStream:WriteAsync");
            cancelToken.ThrowIfCancellationRequested();
            if (!_hasHeader)
                throw new InvalidOperationException("Received payload without an RTP header");
            _hasHeader = false;

            unchecked
            {
                _header[2] = (byte)(_nextSeq >> 8);
                _header[3] = (byte)(_nextSeq >> 0);
                _header[4] = (byte)(_nextTimestamp >> 24);
                _header[5] = (byte)(_nextTimestamp >> 16);
                _header[6] = (byte)(_nextTimestamp >> 8);
                _header[7] = (byte)(_nextTimestamp >> 0);
            }
            Buffer.BlockCopy(_header, 0, _buffer, 0, 12); //Copy RTP header from to the buffer
            Buffer.BlockCopy(buffer, offset, _buffer, 12, count);

            _next.WriteHeader(_nextSeq, _nextTimestamp, false);
            await _next.WriteAsync(_buffer, 0, count + 12).ConfigureAwait(false);
        }

        public override async Task FlushAsync(CancellationToken cancelToken)
        {
            await _next.FlushAsync(cancelToken).ConfigureAwait(false);
        }
        public override async Task ClearAsync(CancellationToken cancelToken)
        {
            await _next.ClearAsync(cancelToken).ConfigureAwait(false);
        }
    }
    
    public class BufferedWriteStream : AudioOutStream
    {
        private const int MaxSilenceFrames = 10;

        private struct Frame
        {
            public Frame(byte[] buffer, int bytes)
            {
                Buffer = buffer;
                Bytes = bytes;
            }

            public readonly byte[] Buffer;
            public readonly int Bytes;
        }

        private static readonly byte[] _silenceFrame = new byte[0];

        private readonly DiscordVoiceClient _client;
        private readonly AudioStream _next;
        private readonly CancellationTokenSource _cancelTokenSource;
        private readonly CancellationToken _cancelToken;        
        private readonly ConcurrentQueue<Frame> _queuedFrames;
        private readonly ConcurrentQueue<byte[]> _bufferPool;
        private readonly SemaphoreSlim _queueLock;        
        private readonly int _ticksPerFrame, _queueLength;
        private bool _isPreloaded;
        private int _silenceFrames;

        public BufferedWriteStream(AudioStream next, DiscordVoiceClient client, int bufferMillis, CancellationToken cancelToken, int maxFrameSize = 1500)
        {
            //maxFrameSize = 1275 was too limiting at 128kbps,2ch,60ms
            _next = next;
            _client = client;
            _ticksPerFrame = OpusEncoder.FrameMillis;
            _queueLength = (bufferMillis + (_ticksPerFrame - 1)) / _ticksPerFrame; //Round up

            _cancelTokenSource = new CancellationTokenSource();
            _cancelToken = CancellationTokenSource.CreateLinkedTokenSource(_cancelTokenSource.Token, cancelToken).Token;
            _queuedFrames = new ConcurrentQueue<Frame>();
            _bufferPool = new ConcurrentQueue<byte[]>();
            for (int i = 0; i < _queueLength; i++)
                _bufferPool.Enqueue(new byte[maxFrameSize]); 
            _queueLock = new SemaphoreSlim(_queueLength, _queueLength);
            _silenceFrames = MaxSilenceFrames;

            Run();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _cancelTokenSource.Cancel();
            base.Dispose(disposing);
        }

        private Task Run()
        {
            return Task.Run(async () =>
            {
                try
                {
                    while (!_isPreloaded && !_cancelToken.IsCancellationRequested)
                        await Task.Delay(1).ConfigureAwait(false);

                    long nextTick = Environment.TickCount;
                    ushort seq = 0;
                    uint timestamp = 0;
                    while (!_cancelToken.IsCancellationRequested)
                    {
                        long tick = Environment.TickCount;
                        long dist = nextTick - tick;
                        if (dist <= 0)
                        {
                            Frame frame;
                            if (_queuedFrames.TryDequeue(out frame))
                            {
                                _client.ToggleSpeaking(true);
                                _next.WriteHeader(seq, timestamp, false);
                                await _next.WriteAsync(frame.Buffer, 0, frame.Bytes).ConfigureAwait(false);
                                _bufferPool.Enqueue(frame.Buffer);
                                _queueLock.Release();
                                nextTick += _ticksPerFrame;
                                seq++;
                                timestamp += OpusEncoder.FrameSamplesPerChannel;
                                _silenceFrames = 0;
#if DEBUG
                                Debug.Log($"Sent {frame.Bytes} bytes ({_queuedFrames.Count} frames buffered)");
#endif
                            }
                            else
                            {
                                while ((nextTick - tick) <= 0)
                                {
                                    if (_silenceFrames++ < MaxSilenceFrames)
                                    {
                                        _next.WriteHeader(seq, timestamp, false);
                                        await _next.WriteAsync(_silenceFrame, 0, _silenceFrame.Length).ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        _client.ToggleSpeaking(false);
                                        //TODO: Handle finish
                                        _cancelTokenSource.Cancel();
                                    }
                                    nextTick += _ticksPerFrame;
                                    seq++;
                                    timestamp += OpusEncoder.FrameSamplesPerChannel;
                                }
#if DEBUG
                                Debug.Log($"Buffer underrun");
#endif
                            }
                        }
                        else
                            await Task.Delay((int)(dist)/*, _cancelToken*/).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException) { }
            });
        }

        public override void WriteHeader(ushort seq, uint timestamp, bool missed) { } //Ignore, we use our own timing
        public override async Task WriteAsync(byte[] data, int offset, int count, CancellationToken cancelToken)
        {
            //Debug.Log("BufferedWriteStream:WriteAsync");
            if (cancelToken.CanBeCanceled)
                cancelToken = CancellationTokenSource.CreateLinkedTokenSource(cancelToken, _cancelToken).Token;
            else
                cancelToken = _cancelToken;

            await _queueLock.WaitAsync(-1, cancelToken).ConfigureAwait(false);
            byte[] buffer;
            if (!_bufferPool.TryDequeue(out buffer))
            {
#if DEBUG
                Debug.Log($"Buffer overflow"); //Should never happen because of the queueLock
#endif
                return;
            }
            Buffer.BlockCopy(data, offset, buffer, 0, count);
            _queuedFrames.Enqueue(new Frame(buffer, count));
            if (!_isPreloaded && _queuedFrames.Count == _queueLength)
            {
#if DEBUG
                Debug.Log($"Preloaded");
#endif
                _isPreloaded = true;
            }
        }

        public override async Task FlushAsync(CancellationToken cancelToken)
        {
            while (true)
            {
                cancelToken.ThrowIfCancellationRequested();
                if (_queuedFrames.Count == 0)
                    return;
                await Task.Delay(250, cancelToken).ConfigureAwait(false);
            }
        }
        public override Task ClearAsync(CancellationToken cancelToken)
        {
            Frame frame;
            do
                cancelToken.ThrowIfCancellationRequested();
            while (_queuedFrames.TryDequeue(out frame));
            return Task.Delay(0);
        }
    }
    
    internal enum OpusCtl : int
    {
        SetBitrate = 4002,
        SetBandwidth = 4008,
        SetInbandFEC = 4012,
        SetPacketLossPercent = 4014,
        SetSignal = 4024
    }
    
    internal enum OpusApplication : int
    {
        Voice = 2048,
        MusicOrMixed = 2049,
        LowLatency = 2051
    }
    
    internal enum OpusSignal : int
    {
        Auto = -1000,
        Voice = 3001,
        Music = 3002,
    }
    
    internal unsafe class OpusEncoder : OpusConverter
    {
        [DllImport("opus_egpv", EntryPoint = "opus_encoder_create", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CreateEncoder(int Fs, int channels, int application, out OpusError error);
        [DllImport("opus_egpv", EntryPoint = "opus_encoder_destroy", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DestroyEncoder(IntPtr encoder);
        [DllImport("opus_egpv", EntryPoint = "opus_encode", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Encode(IntPtr st, byte* pcm, int frame_size, byte* data, int max_data_bytes);
        [DllImport("opus_egpv", EntryPoint = "opus_encoder_ctl", CallingConvention = CallingConvention.Cdecl)]
        private static extern OpusError EncoderCtl(IntPtr st, OpusCtl request, int value);
        
        public AudioApplication Application { get; }
        public int BitRate { get;}

        public OpusEncoder(int bitrate, AudioApplication application, int packetLoss)
        {
            if (bitrate < 1 || bitrate > DiscordClient.MaxBitrate)
                throw new ArgumentOutOfRangeException(nameof(bitrate));

            Application = application;
            BitRate = bitrate;

            OpusApplication opusApplication;
            OpusSignal opusSignal;
            switch (application)
            {
                case AudioApplication.Mixed:
                    opusApplication = OpusApplication.MusicOrMixed;
                    opusSignal = OpusSignal.Auto;
                    break;
                case AudioApplication.Music:
                    opusApplication = OpusApplication.MusicOrMixed;
                    opusSignal = OpusSignal.Music;
                    break;
                case AudioApplication.Voice:
                    opusApplication = OpusApplication.Voice;
                    opusSignal = OpusSignal.Voice;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(application));
            }

            OpusError error;
            _ptr = CreateEncoder(SamplingRate, Channels, (int)opusApplication, out error);
            CheckError(error);
            CheckError(EncoderCtl(_ptr, OpusCtl.SetSignal, (int)opusSignal));
            CheckError(EncoderCtl(_ptr, OpusCtl.SetPacketLossPercent, packetLoss)); //%
            CheckError(EncoderCtl(_ptr, OpusCtl.SetInbandFEC, 1)); //True
            CheckError(EncoderCtl(_ptr, OpusCtl.SetBitrate, bitrate));
        }

        public unsafe int EncodeFrame(byte[] input, int inputOffset, byte[] output, int outputOffset)
        {
            int result = 0;
            fixed (byte* inPtr = input)
            fixed (byte* outPtr = output)
                result = Encode(_ptr, inPtr + inputOffset, FrameSamplesPerChannel, outPtr + outputOffset, output.Length - outputOffset);
            CheckError(result);
            return result;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (_ptr != IntPtr.Zero)
                    DestroyEncoder(_ptr);
                base.Dispose(disposing);
            }
        }
    }
    
    internal abstract class OpusConverter : IDisposable
    {
        protected IntPtr _ptr;

        public const int SamplingRate = 48000;
        public const int Channels = 2;
        public const int FrameMillis = 20;

        public const int SampleBytes = sizeof(short) * Channels;

        public const int FrameSamplesPerChannel = SamplingRate / 1000 * FrameMillis;
        public const int FrameSamples = FrameSamplesPerChannel * Channels;
        public const int FrameBytes = FrameSamplesPerChannel * SampleBytes;
        
        protected bool _isDisposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
                _isDisposed = true;
        }
        ~OpusConverter()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected static void CheckError(int result)
        {
            if (result < 0)
                throw new Exception($"Opus Error: {(OpusError)result}");
        }
        protected static void CheckError(OpusError error)
        {
            if ((int)error < 0)
                throw new Exception($"Opus Error: {error}");
        }
    }
    
    internal enum OpusError : int
    {
        OK = 0,
        BadArg = -1,
        BufferToSmall = -2,
        InternalError = -3,
        InvalidPacket = -4,
        Unimplemented = -5,
        InvalidState = -6,
        AllocFail = -7
    }
    
    public class OpusEncodeStream : AudioOutStream
    {
        public const int SampleRate = 48000;

        private readonly AudioStream _next;
        private readonly OpusEncoder _encoder;
        private readonly byte[] _buffer;
        private int _partialFramePos;
        private ushort _seq;
        private uint _timestamp;
        
        public OpusEncodeStream(AudioStream next, int bitrate, AudioApplication application, int packetLoss)
        {
            _next = next;
            _encoder = new OpusEncoder(bitrate, application, packetLoss);
            _buffer = new byte[OpusConverter.FrameBytes];
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancelToken)
        {
            //Debug.Log("OpusEncodeStream:WriteAsync");
            //Assume threadsafe
            while (count > 0)
            {
                if (_partialFramePos == 0 && count >= OpusConverter.FrameBytes)
                {
                    //We have enough data and no partial frames. Pass the buffer directly to the encoder
                    int encFrameSize = _encoder.EncodeFrame(buffer, offset, _buffer, 0);
                    _next.WriteHeader(_seq, _timestamp, false);
                    await _next.WriteAsync(_buffer, 0, encFrameSize, cancelToken).ConfigureAwait(false);

                    offset += OpusConverter.FrameBytes;
                    count -= OpusConverter.FrameBytes;
                    _seq++;
                    _timestamp += OpusConverter.FrameSamplesPerChannel;
                }
                else if (_partialFramePos + count >= OpusConverter.FrameBytes)
                {
                    //We have enough data to complete a previous partial frame.
                    int partialSize = OpusConverter.FrameBytes - _partialFramePos;
                    Buffer.BlockCopy(buffer, offset, _buffer, _partialFramePos, partialSize);
                    int encFrameSize = _encoder.EncodeFrame(_buffer, 0, _buffer, 0);
                    _next.WriteHeader(_seq, _timestamp, false);
                    await _next.WriteAsync(_buffer, 0, encFrameSize, cancelToken).ConfigureAwait(false);

                    offset += partialSize;
                    count -= partialSize;
                    _partialFramePos = 0;
                    _seq++;
                    _timestamp += OpusConverter.FrameSamplesPerChannel;
                }
                else
                {
                    //Not enough data to build a complete frame, store this part for later
                    Buffer.BlockCopy(buffer, offset, _buffer, _partialFramePos, count);
                    _partialFramePos += count;
                    break;
                }
            }
        }

        /* //Opus throws memory errors on bad frames
        public override async Task FlushAsync(CancellationToken cancelToken)
        {
            try
            {
                int encFrameSize = _encoder.EncodeFrame(_partialFrameBuffer, 0, _partialFramePos, _buffer, 0);
                base.Write(_buffer, 0, encFrameSize);
            }
            catch (Exception) { } //Incomplete frame
            _partialFramePos = 0;
            await base.FlushAsync(cancelToken).ConfigureAwait(false);
        }*/

        public override async Task FlushAsync(CancellationToken cancelToken)
        {
            await _next.FlushAsync(cancelToken).ConfigureAwait(false);
        }
        public override async Task ClearAsync(CancellationToken cancelToken)
        {
            await _next.ClearAsync(cancelToken).ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
                _encoder.Dispose();
        }
    }
    
    public static unsafe class SecretBox
    {
        public static int Encrypt(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, byte[] nonce, byte[] secret)
        {
            fixed (byte* inPtr = input)
            fixed (byte* outPtr = output)
            {
                //Debug.Log($"Libsodium before: {string.Join(", ", output)}");             
                int error = NativeLibsodium.crypto_secretbox_easy(outPtr + outputOffset, inPtr + inputOffset, inputLength, nonce, secret);
                //Debug.Log($"Libsodium after: {error}, {string.Join(", ", output)}");
                if (error != 0)
                {
                    throw new Exception($"Sodium Error: {error}");
                }

                return inputLength + 16;
            }
        }
    }
}


