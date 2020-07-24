using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using PgpCore;
using Tides.Util;

namespace Tides.Models.Auth.Types
{
    public class OpenPGPAuthHandler : IAuthTypeHandler
    {
        public AuthType Type { get; } = AuthType.OpenPGP;

        public AuthKeyFormat Format => AuthKeyFormat.PEM;

        public HashSet<HandshakeMethod> HandshakeMethodsSupported { get; } = new HashSet<HandshakeMethod>();

        public async Task DecryptAndVerifyStreamAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey, PublicKey publicKey, Func<string> passwordFinder)
        {
            using var pgp = new PGP();
            using var publicKeyStream = GetPublicKey(publicKey);
            using var privateKeyStream = GetPrivateKey(privateKey);
            await pgp.DecryptStreamAndVerifyAsync(inputStream, outputStream, publicKeyStream, privateKeyStream, null);
        }

        public async Task DecryptStreamAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey, Func<string> passwordFinder)
        {
            using var pgp = new PGP();
            using var privateKeyStream = GetPrivateKey(privateKey);
		    await pgp.DecryptStreamAsync(inputStream, outputStream, privateKeyStream, null);
        }

        public async Task EncryptStreamAndSignAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey, PublicKey publicKey, Func<string> passwordFinder)
        {
            using var pgp = new PGP();
            using var publicKeyStream = GetPublicKey(publicKey);
            using var privateKeyStream = GetPrivateKey(privateKey);
            await pgp.EncryptStreamAndSignAsync(inputStream, outputStream, privateKeyStream, publicKeyStream, null);
        }

        public async Task EncryptStreamAsync(Stream inputStream, Stream outputStream, PublicKey publicKey)
        {
            using var pgp = new PGP();
            using var publicKeyStream = GetPublicKey(publicKey);
		    await pgp.EncryptStreamAsync(inputStream, outputStream, publicKeyStream, true, true);
        }

        public async Task SignStreamAsync(Stream inputStream, Stream outputStream, PrivateKey privateKey, Func<string> passwordFinder)
        {
            using var pgp = new PGP();
            using var privateKeyStream = GetPrivateKey(privateKey);
		    await pgp.SignStreamAsync(inputStream, outputStream, privateKeyStream, null, true, true);
        }

        public async Task<bool> VerifyStreamAsync(Stream inputStream, Stream outputStream, PublicKey publicKey)
        {
            using var pgp = new PGP();
            using var publicKeyStream = GetPublicKey(publicKey);
            return await pgp.VerifyStreamAsync(inputStream, publicKeyStream);
        }

        public async Task<KeyPair> GenerateKey(HandshakeMethod method)
        {
            var tmpFilePublic = System.IO.Path.GetTempFileName();
            var tmpFilePrivate = System.IO.Path.GetTempFileName();
            using var pgp = new PGP();
            await pgp.GenerateKeyAsync(tmpFilePublic, tmpFilePrivate);
            var publicKey = await File.ReadAllTextAsync(tmpFilePublic);
            var privateKey = await File.ReadAllTextAsync(tmpFilePrivate);
            return new KeyPair()
            {
                Private = new PrivateKey()
                {
                    Format = AuthKeyFormat.PEM,
                    Type = Type,
                    Value = privateKey,
                    WhenAdded = DateTime.UtcNow,
                },
                Public = new PublicKey()
                {
                    Format = AuthKeyFormat.PEM,
                    Type = Type,
                    Value = publicKey.Trim(),
                    WhenAdded = DateTime.UtcNow,
                },
            };
        }

        public bool HandshakeMethodSupported(HandshakeMethod mehod)
        {
            return false;
        }

        private Stream GetPublicKey(PublicKey auth) => new MemoryStream(Encoding.UTF8.GetBytes(auth.Value));

        private Stream GetPrivateKey(PrivateKey auth) => new MemoryStream(Encoding.UTF8.GetBytes(auth.Value.Split("\n\n")[0]));

        public string ComputeFingerPrint(PublicKey key, HashAlgorithm alg)
        {
            var raw = key.Value ?? throw new ArgumentNullException("value");
            raw = RegexHelper.OpenPGPKeyRegex.Match(raw).Groups["base64"].Value.Replace("\n", "").Trim();
            var bytes = Encoding.ASCII.GetBytes(raw);
            var hashBytes = alg.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace('-', ':');
        }
    }
}