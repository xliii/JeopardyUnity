using System.Threading;
using System.Threading.Tasks;

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
		cancelToken.ThrowIfCancellationRequested();
		await _client.SendAsync(buffer, offset, count);
	}
}