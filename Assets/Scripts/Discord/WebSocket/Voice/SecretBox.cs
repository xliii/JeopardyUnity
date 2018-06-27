using System;
using unity.libsodium;

public static unsafe class SecretBox
{
	public static int Encrypt(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, byte[] nonce, byte[] secret)
	{
		fixed (byte* inPtr = input)
		fixed (byte* outPtr = output)
		{
			var error = NativeLibsodium.crypto_secretbox_easy(outPtr + outputOffset, inPtr + inputOffset, inputLength, nonce, secret);
			if (error != 0)
			{
				throw new Exception($"Sodium Error: {error}");
			}

			return inputLength + 16;
		}
	}
}