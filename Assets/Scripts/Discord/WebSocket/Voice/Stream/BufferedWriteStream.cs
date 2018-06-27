﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using UnityEngine;

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
                                //Debug.Log($"Sent {frame.Bytes} bytes ({_queuedFrames.Count} frames buffered)");
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