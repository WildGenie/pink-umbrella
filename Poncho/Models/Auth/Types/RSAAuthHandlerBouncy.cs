using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
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
            await Decrypt(inputStream, outputStream, privateKey, passwordFinder);
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

            // Func<RSACryptoServiceProvider, RsaKeyParameters, RSACryptoServiceProvider> MakePublicRCSP = (RSACryptoServiceProvider rcsp, RsaKeyParameters rkp) =>
            // {
            //     RSAParameters rsaParameters = DotNetUtilities.ToRSAParameters(rkp);
            //     rcsp.ImportParameters(rsaParameters);
            //     return rsaKey;
            // };

            // var cspParameters = new CspParameters();
            // var rsaKey = new RSACryptoServiceProvider(cspParameters);
            // PemReader reader = new PemReader(new StringReader(privateKey.Value));
            // object kp = reader.ReadObject();
            // var rkp = (RsaPrivateCrtKeyParameters)(((AsymmetricCipherKeyPair)kp).Private);
            // RSAParameters rsaParameters = DotNetUtilities.ToRSAParameters(rkp);
            // rsaKey.ImportParameters(rsaParameters);

            // AsymmetricKeyParameter asymmetricKeyParameter = PublicKeyFactory.CreateKey(Convert.FromBase64String(privateKey.Value));
            // RsaKeyParameters rsaKeyParameters = (RsaKeyParameters) asymmetricKeyParameter;
            // RSAParameters rsaParameters = new RSAParameters();
            // rsaParameters.Modulus = rsaKeyParameters.Modulus.ToByteArrayUnsigned();
            // rsaParameters.Exponent = rsaKeyParameters.Exponent.ToByteArrayUnsigned();
            // RSACryptoServiceProvider _rsa = new RSACryptoServiceProvider();
            // _rsa.ImportParameters(rsaParameters);

            await Decrypt(inputStream, outputStream, privateKey, passwordFinder);
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
            var randomGenerator = new CryptoApiRandomGenerator();
            var secureRandom = new SecureRandom(randomGenerator);
            var keyGenerationParameters = new KeyGenerationParameters(secureRandom, 1024);

            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            var pair = keyPairGenerator.GenerateKeyPair();

            string privateKey;
            using (var writer = new StringWriter())
            {
                var pemWriter = new PemWriter(writer);
                pemWriter.WriteObject(pair.Private);
                pemWriter.Writer.Flush();
                privateKey = writer.ToString();
            }

            string publicKey;
            using (var writer = new StringWriter())
            {
                var pemWriter = new PemWriter(writer);
                pemWriter.WriteObject(pair.Public);
                pemWriter.Writer.Flush();
                publicKey = writer.ToString();
            }

            return Task.FromResult(new KeyPair()
            {
                Public = new PublicKey()
                {
                    Value = publicKey,
                    Format = Format,
                    Type = Type,
                },
                Private = new PrivateKey()
                {
                    Value = privateKey,
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
            var raw = key.Value ?? throw new ArgumentNullException("value");
            raw = RegexHelper.RSAKeyRegex.Match(raw).Groups["base64"].Value.Trim();
            var bytes = Convert.FromBase64String(raw);
            var hashBytes = alg.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace('-', ':');
        }
    }
}