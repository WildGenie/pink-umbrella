using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Poncho.Util;

namespace Poncho.Models.Auth.Types
{
    public class RSAAuthHandlerBouncy : RSAAuthHandler
    {
        public RSAAuthHandlerBouncy(): base() { }

        public override AuthKeyFormat Format => AuthKeyFormat.PEM;
        
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

        private async Task Decrypt(Stream inputStream, Stream outputStream, PrivateKey privateKey, Func<string> passwordFinder)
        {
            if (privateKey == null)
            {
                throw new ArgumentNullException(nameof(privateKey));
            }
            var encryptEngine = new Pkcs1Encoding(new RsaEngine());
            var keyReader = new StringReader(privateKey.Value);
            var pr = passwordFinder != null ? new PemReader(keyReader, new PasswordFinder { Get = passwordFinder }) : new PemReader(keyReader);
            var keyParameter = (AsymmetricCipherKeyPair)pr.ReadObject();
            encryptEngine.Init(false, keyParameter?.Private ?? throw new ArgumentException("Private key was null"));
            await ProcessStream(inputStream, outputStream, encryptEngine);
        }

        private async Task Encrypt(Stream inputStream, Stream outputStream, PublicKey publicKey)
        {
            if (publicKey == null)
            {
                throw new ArgumentNullException(nameof(publicKey));
            }
            var encryptEngine = new Pkcs1Encoding(new RsaEngine());
            var pr = new PemReader(new StringReader(publicKey.Value));
            var keyParameter = (RsaKeyParameters)pr.ReadObject();
            if (keyParameter != null)
            {
                encryptEngine.Init(true, keyParameter);
            }
            else
            {
                throw new Exception();
            }
            await ProcessStream(inputStream, outputStream, encryptEngine);
        }

        private static async Task ProcessStream(Stream inputStream, Stream outputStream, Pkcs1Encoding encryptEngine)
        {
            int numRead = 0;
            var buffer = new byte[encryptEngine.GetInputBlockSize()];
            var totalWritten = 0;
            while (true)
            {
                numRead = await inputStream.ReadAsync(buffer);
                if (numRead > 0)
                {
                    var outBuffer = encryptEngine.ProcessBlock(buffer, 0, numRead);
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
    }
}