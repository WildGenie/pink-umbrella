using System.IO;
using System.Security.Cryptography;

namespace Tides.Models.Crypto
{
    public class AesHelper
    {
        public readonly int Size = 128;
        public readonly byte[] IV = new byte[16];
        public readonly byte[] Key = new byte[32];

        public AesHelper()
        {
        }

        public void Randomize()
        {
            if (Size > 0)
            {
                var rng = new RNGCryptoServiceProvider();
                rng.GetBytes(IV);
                rng.GetBytes(Key);
            }
            else
            {
                throw new System.Exception("Size not set");
            }
        }

        public void Encrypt(Stream inputStream, Stream outputStream) => Stream(inputStream, outputStream, CryptoStreamMode.Write);

        public void Decrypt(Stream inputStream, Stream outputStream) => Stream(inputStream, outputStream, CryptoStreamMode.Read);

        public void Stream(Stream inputStream, Stream outputStream, CryptoStreamMode streamMode)
        {
            var crypt = Aes.Create();
            var hash = MD5.Create();
            crypt.BlockSize = Size;
            crypt.Key = Key;
            crypt.IV = IV;

            if (streamMode == CryptoStreamMode.Read)
            {
                inputStream.Read(IV);
            }
            else
            {
                outputStream.Write(IV);
            }

            var cryptoStream = new CryptoStream(outputStream, crypt.CreateEncryptor(), streamMode);
            if (streamMode == CryptoStreamMode.Read)
            {
                cryptoStream.CopyTo(inputStream);
            }
            else
            {
                inputStream.CopyTo(cryptoStream);
            }
        }
    }
}