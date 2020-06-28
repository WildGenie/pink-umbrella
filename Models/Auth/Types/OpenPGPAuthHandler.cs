using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PgpCore;

namespace PinkUmbrella.Models.Auth.Types
{
    public class OpenPGPAuthHandler : IAuthTypeHandler
    {
        public AuthType Type { get; } = AuthType.OpenPGP;

        public HashSet<HandshakeMethod> HandshakeMethodsSupported => throw new System.NotImplementedException();

        public async Task DecryptAndVerifyStreamAsync(Stream inputStream, Stream outputStream, AuthKey auth)
        {
            using var pgp = new PGP();
	        // Decrypt stream and verify
            using var inputFileStream = new FileStream(@"C:\TEMP\Content\encryptedAndSigned.pgp", FileMode.Open);
            using var outputFileStream = File.Create(@"C:\TEMP\Content\decryptedAndVerified.txt");
            using var publicKeyStream = new FileStream(@"C:\TEMP\Keys\public.asc", FileMode.Open);
            using var privateKeyStream = new FileStream(@"C:\TEMP\Keys\private.asc", FileMode.Open);
            await pgp.DecryptStreamAndVerifyAsync(inputFileStream, outputFileStream, publicKeyStream, privateKeyStream, "password");
        }

        public async Task DecryptStreamAsync(Stream inputStream, Stream outputStream, AuthKey auth)
        {
            using var pgp = new PGP();
            // Decrypt stream
            using var inputFileStream = new FileStream(@"C:\TEMP\Content\encrypted.pgp", FileMode.Open);
            using var outputFileStream = File.Create(@"C:\TEMP\Content\decrypted.txt");
            using var privateKeyStream = new FileStream(@"C:\TEMP\Keys\private.asc", FileMode.Open);
		    await pgp.DecryptStreamAsync(inputFileStream, outputFileStream, privateKeyStream, "password");
        }

        public async Task EncryptStreamAndSignAsync(Stream inputStream, Stream outputStream, AuthKey auth)
        {
            using var pgp = new PGP();
            // Encrypt and sign stream
            using var inputFileStream = new FileStream(@"C:\TEMP\Content\content.txt", FileMode.Open);
            using var outputFileStream = File.Create(@"C:\TEMP\Content\encryptedAndSigned.pgp");
            using var publicKeyStream = new FileStream(@"C:\TEMP\Keys\public.asc", FileMode.Open);
            using var privateKeyStream = new FileStream(@"C:\TEMP\Keys\private.asc", FileMode.Open);
            await pgp.EncryptStreamAndSignAsync(inputFileStream, outputFileStream, publicKeyStream, privateKeyStream, "password", true, true);
        }

        public async Task EncryptStreamAsync(Stream inputStream, Stream outputStream, AuthKey auth)
        {
            using var pgp = new PGP();
	        // Encrypt stream
            using var inputFileStream = new FileStream(@"C:\TEMP\Content\content.txt", FileMode.Open);
            using var outputFileStream = File.Create(@"C:\TEMP\Content\encrypted.pgp");
            using var publicKeyStream = new FileStream(@"C:\TEMP\Keys\public.asc", FileMode.Open);
		    await pgp.EncryptStreamAsync(inputFileStream, outputFileStream, publicKeyStream, true, true);
        }

        public async Task<AuthKey> GenerateKey(HandshakeMethod method)
        {
            using var pgp = new PGP();
            await pgp.GenerateKeyAsync(@"C:\TEMP\Keys\public.asc", @"C:\TEMP\Keys\private.asc", "email@email.com", "password");
            return new AuthKey()
            {
                Format = AuthKeyFormat.Raw,
                Type = Type,
                Value = "",
            };
        }

        public bool HandshakeMethodSupported()
        {
            return false;
        }

        public async Task SignStreamAsync(Stream inputStream, Stream outputStream, AuthKey auth)
        {
            using var pgp = new PGP();
            // Sign stream
            using var inputFileStream = new FileStream(@"C:\TEMP\Content\content.txt", FileMode.Open);
            using var outputFileStream = File.Create(@"C:\TEMP\Content\signed.pgp");
            using var privateKeyStream = new FileStream(@"C:\TEMP\Keys\private.asc", FileMode.Open);
		    await pgp.SignStreamAsync(inputFileStream, outputFileStream, privateKeyStream, "password", true, true);
        }

        public async Task<bool> VerifyStreamAsync(Stream inputStream, Stream outputStream, AuthKey auth)
        {
            using var pgp = new PGP();
            // Verify stream
            using var inputFileStream = new FileStream(@"C:\TEMP\Content\content.txt", FileMode.Open);
            using var publicKeyStream = new FileStream(@"C:\TEMP\Keys\private.asc", FileMode.Open);
            return await pgp.VerifyStreamAsync(inputFileStream, publicKeyStream);
        }
    }
}