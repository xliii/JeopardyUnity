﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;

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

        private byte[] SecretKey => _client.SecretKey;

        public override async Task FlushAsync(CancellationToken cancelToken)
        {
            await _next.FlushAsync(cancelToken).ConfigureAwait(false);
        }
        public override async Task ClearAsync(CancellationToken cancelToken)
        {
            await _next.ClearAsync(cancelToken).ConfigureAwait(false);
        }
    }