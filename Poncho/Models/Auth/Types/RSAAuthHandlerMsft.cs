using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Poncho.Util;

namespace Poncho.Models.Auth.Types
{
    public class RSAAuthHandlerMsft : RSAAuthHandler
    {
        public RSAAuthHandlerMsft(): base() { }

        public override AuthKeyFormat Format => AuthKeyFormat.Msft;

        public override async Task DecryptAndVerifyStreamAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey, PublicKey publicKey, Func<string> passwordFinder)
        {
            int numRead = 0;

            var rsaCsp = new RSACryptoServiceProvider();
            rsaCsp.ImportRSAPrivateKey(Convert.FromBase64String(privateKey.Value), out numRead);

            var buffer = new byte[rsaCsp.KeySize / 8];
            var totalWritten = 0;
            while (true)
            {
                numRead = await inputStream.ReadAsync(buffer);
                if (numRead > 0)
                {
                    var todo = new byte[numRead];
                    Array.Copy(buffer, todo, numRead);
                    var outBuffer = rsaCsp.Decrypt(todo, RSAEncryptionPadding.Pkcs1);
                    totalWritten += outBuffer.Length;
                    await outputStream.WriteAsync(outBuffer);
                }
                else
                {
                    await outputStream.FlushAsync();
                    break;
                }
            }
        }

        private async Task Encrypt(Stream inputStream, Stream outputStream, PublicKey publicKey)
        {
            if (publicKey == null)
            {
                throw new ArgumentNullException(nameof(publicKey));
            }
            var _rsa = new RSACryptoServiceProvider();
            _rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey.Value), out var numRead);
            await ProcessStream(inputStream, outputStream, _rsa);
            return;
        }

        private static async Task ProcessStream(Stream inputStream, Stream outputStream, RSACryptoServiceProvider rsa)
        {
            int numRead = 0;
            var buffer = new byte[rsa.KeySize / 8 - 11];
            var totalWritten = 0;
            while (true)
            {
                numRead = await inputStream.ReadAsync(buffer);
                if (numRead > 0)
                {
                    var todo = numRead == buffer.Length ? buffer : new byte[numRead];
                    if (!Object.ReferenceEquals(todo, buffer))
                    {
                        Array.Copy(buffer, todo, numRead);
                    }
                    var outBuffer = rsa.Encrypt(todo, RSAEncryptionPadding.Pkcs1);
                    totalWritten += outBuffer.Length;
                    await outputStream.WriteAsync(outBuffer);
                }
                else
                {
                    await outputStream.FlushAsync();
                    break;
                }
            }
            await outputStream.FlushAsync();
            outputStream.Position = 0;
        }

        public override async Task DecryptStreamAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey, Func<string> passwordFinder)
        {
            AsymmetricKeyParameter asymmetricKeyParameter = PublicKeyFactory.CreateKey(Convert.FromBase64String(privateKey.Value));
            RsaKeyParameters rsaKeyParameters = (RsaKeyParameters) asymmetricKeyParameter;
            RSAParameters rsaParameters = new RSAParameters();
            rsaParameters.Modulus = rsaKeyParameters.Modulus.ToByteArrayUnsigned();
            rsaParameters.Exponent = rsaKeyParameters.Exponent.ToByteArrayUnsigned();
            RSACryptoServiceProvider _rsa = new RSACryptoServiceProvider();
            _rsa.ImportParameters(rsaParameters);

            int numRead = 0;

            // var _rsa = new RSACryptoServiceProvider();
            // _rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey.Value), out numRead);


            var buffer = new byte[_rsa.KeySize / 8 - 11];
            var totalWritten = 0;
            while (true)
            {
                numRead = await inputStream.ReadAsync(buffer);
                if (numRead > 0)
                {
                    var todo = new byte[numRead];
                    Array.Copy(buffer, todo, numRead);
                    var outBuffer = _rsa.Decrypt(todo, RSAEncryptionPadding.OaepSHA1);
                    totalWritten += outBuffer.Length;
                    await outputStream.WriteAsync(outBuffer);
                }
                else
                {
                    await outputStream.FlushAsync();
                    break;
                }
            }
        }

        public override Task EncryptStreamAndSignAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey, PublicKey publicKey, Func<string> passwordFinder)
        {
            return EncryptStreamAsync(inputStream, outputStream, publicKey);
        }

        public override async Task EncryptStreamAsync(Stream inputStream, Stream outputStream, PublicKey auth)
        {
            await Encrypt(inputStream, outputStream, auth);
        }

        public override Task<KeyPair> GenerateKey(HandshakeMethod method)
        {
            var _rsa = new RSACryptoServiceProvider();
            var rsaKeyInfo = _rsa.ExportParameters(true);
            return Task.FromResult(new KeyPair()
            {
                Public = new PublicKey()
                {
                    Value = Convert.ToBase64String(_rsa.ExportSubjectPublicKeyInfo()),
                    Format = Format,
                    Type = Type,
                },
                Private = new PrivateKey()
                {
                    Value = Convert.ToBase64String(_rsa.ExportRSAPrivateKey()),
                    Format = Format,
                    Type = Type,
                },
            });
        }

        public override Task SignStreamAsync(Stream inputStream, Stream outputStream, PrivateKey auth, Func<string> passwordFinder)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> VerifyStreamAsync(Stream inputStream, Stream outputStream, PublicKey publicKey)
        {
            throw new NotImplementedException();
        }

        public override string ComputeFingerPrint(PublicKey key, HashAlgorithm alg)
        {
            string raw = key.Value ?? throw new ArgumentNullException("value");
            raw = RegexHelper.RSAKeyRegex.Match(raw).Groups["base64"].Value.Trim();
            var bytes = Convert.FromBase64String(raw);
            var hashBytes = alg.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace('-', ':');
        }
    }
}