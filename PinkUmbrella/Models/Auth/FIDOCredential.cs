using System;
using System.Linq;
using Fido2NetLib.Development;
using Fido2NetLib.Objects;

namespace PinkUmbrella.Models.Auth
{
    public class FIDOCredential
    {
        public FIDOCredential(StoredCredential credential)
        {
            this.Copy(credential);
        }

        public FIDOCredential() { }

        public long Id { get; set; }

        public int UserId { get; set; }

        public byte[] PublicKey { get; set; }

        public Guid AaGuid { get; set; }
        
        public string CredType { get; set; }

        public PublicKeyCredentialType Type { get; set; }

        public DateTime WhenCreated { get; set; }
        
        public uint SignatureCounter { get; set; }

        public string TransportTypes { get; set; }

        public void Copy(StoredCredential cred)
        {
            this.AaGuid = cred.AaGuid;
            this.CredType = cred.CredType;
            this.PublicKey = cred.PublicKey;
            this.WhenCreated = cred.RegDate;
            this.SignatureCounter = cred.SignatureCounter;
            this.UserId = BitConverter.ToInt32(cred.UserId);
            this.TransportTypes = string.Join(',', cred.Descriptor.Transports);;
            this.Type = cred.Descriptor.Type ?? throw new Exception(); // PublicKeyCredentialType.PublicKey
        }

        public StoredCredential UnFurl()
        {
            return new StoredCredential()
            {
                AaGuid = AaGuid,
                CredType = CredType,
                PublicKey = PublicKey,
                RegDate = WhenCreated,
                SignatureCounter = SignatureCounter,
                Descriptor = new PublicKeyCredentialDescriptor(BitConverter.GetBytes(Id))
                {
                    Transports = TransportTypes.Split(',').Select(d => Enum.Parse(typeof(AuthenticatorTransport), d)).Cast<AuthenticatorTransport>().ToArray(),
                    Id = BitConverter.GetBytes(Id),
                    Type = Type,
                },
                UserHandle = BitConverter.GetBytes(UserId),
                UserId = BitConverter.GetBytes(UserId),
            };
        }
    }
}