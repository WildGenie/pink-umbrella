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
using PinkUmbrella.Repositories;

namespace PinkUmbrella.Models.Auth.Types
{
    public class RSAAuthHandler : IAuthTypeHandler
    {
        private readonly StringRepository _strings;

        public RSAAuthHandler(StringRepository strings)
        {
            _strings = strings;
        }
        
        public AuthType Type { get; } = AuthType.RSA;

        public HashSet<HandshakeMethod> HandshakeMethodsSupported => throw new System.NotImplementedException();

        public async Task DecryptAndVerifyStreamAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey, PublicKey publicKey)
        {
            var pr = new PemReader(new StringReader(publicKey.Value));
            var asymmetricKeyParameter = (AsymmetricKeyParameter)pr.ReadObject();
            var rsaKeyParameters = (RsaKeyParameters)asymmetricKeyParameter;
            var rsaParams = DotNetUtilities.ToRSAParameters(rsaKeyParameters);
            var rsaCsp = new RSACryptoServiceProvider();
            rsaCsp.ImportParameters(rsaParams);

            //AsymmetricKeyParameter asymmetricKeyParameter = PublicKeyFactory.CreateKey(Convert.FromBase64String(privateKey.Value));
            
            //var original = ToPEM(publicKey.Value);
            //PemReader pr = new PemReader(new StringReader(original));
            //AsymmetricCipherKeyPair KeyPair = (AsymmetricKeyParameter)pr.ReadObject();
            //RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)KeyPair.Private); 
            //rsaCsp.ImportParameters(rsaParams);

            int numRead = 0;

            // var _rsa = new RSACryptoServiceProvider();
            // _rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey.Value), out numRead);


            var buffer = new byte[rsaCsp.KeySize / 8 - 11];
            var totalWritten = 0;
            while (true)
            {
                numRead = await inputStream.ReadAsync(buffer);
                if (numRead > 0)
                {
                    var todo = new byte[numRead];
                    Array.Copy(buffer, todo, numRead);
                    var outBuffer = rsaCsp.Decrypt(todo, RSAEncryptionPadding.OaepSHA1);
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

        public async Task DecryptStreamAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey)
        {
            // var original = ToPrivatePEM(privateKey.Value);
            // var encryptEngine = new Pkcs1Encoding(new RsaEngine());
            // using (var txtreader = new StringReader(original))
            // {
            //     var keyParameter = (AsymmetricKeyParameter)new PemReader(txtreader).ReadObject();
            //     encryptEngine.Init(true, keyParameter);
            // }

            // int numRead = 0;
            // var buffer = new byte[_rsa.KeySize / 8 - 11];
            // var totalWritten = 0;
            // while (true)
            // {
            //     numRead = await inputStream.ReadAsync(buffer);
            //     if (numRead > 0)
            //     {
            //         var outBuffer = encryptEngine.ProcessBlock(buffer, 0, numRead);
            //         totalWritten += outBuffer.Length;
            //         await outputStream.WriteAsync(outBuffer);
            //     }
            //     else
            //     {
            //         await outputStream.FlushAsync();
            //         break;
            //     }
            // }
            // await outputStream.FlushAsync();
            
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

        public Task EncryptStreamAndSignAsync(Stream inputStream, Stream outputStream, PublicKey publicKey, PrivateKey privateKey)
        {
            return Task.CompletedTask;
        }

        public Task EncryptStreamAndSignAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey, PublicKey publicKey)
        {
            return EncryptStreamAsync(inputStream, outputStream, publicKey);
        }

        public async Task EncryptStreamAsync(Stream inputStream, Stream outputStream, PublicKey auth)
        {
            var pr = new PemReader(new StringReader(auth.Value));
            var encryptEngine = new Pkcs1Encoding(new RsaEngine());
            var keyParameter = (AsymmetricKeyParameter)pr.ReadObject();
            encryptEngine.Init(true, keyParameter);

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
        }

        private byte[] ToX509(string value) => Encoding.ASCII.GetBytes(ToPEM(value));

        private string ToPEM(string value)
        {
            var ret = new StringBuilder();
            ret.Append("-----BEGIN RSA PUBLIC KEY-----\n");
            if (_strings.RSAKeyRegex.Match(value).Groups.TryGetValue("base64", out var base64))
            {
                ret.Append(base64.Value);
            }
            else
            {
                ret.Append(value);
            }
            ret.Append("\n-----END RSA PUBLIC KEY-----\n");
            return ret.ToString();
        }

        public Task<KeyPair> GenerateKey(AuthKeyFormat format, HandshakeMethod method)
        {
            var _rsa = new RSACryptoServiceProvider();
            var rsaKeyInfo = _rsa.ExportParameters(true);
            return Task.FromResult(new KeyPair()
            {
                Public = new PublicKey()
                {
                    Value = Convert.ToBase64String(_rsa.ExportSubjectPublicKeyInfo()),
                    Format = format,
                    Type = Type,
                },
                Private = new PrivateKey()
                {
                    Value = Convert.ToBase64String(_rsa.ExportRSAPrivateKey()),
                    Format = format,
                    Type = Type,
                },
            });
        }

        public bool HandshakeMethodSupported(HandshakeMethod method)
        {
            return false;
        }

        public Task SignStreamAsync(Stream inputStream, Stream outputStream, PrivateKey auth)
        {
            throw new NotImplementedException();
        }

        public Task<bool> VerifyStreamAsync(Stream inputStream, Stream outputStream, PublicKey publicKey)
        {
            throw new NotImplementedException();
        }
    }
}