namespace Poncho.Models.Peer
{
    public class RESTPeerClientType : IPeerConnectionType
    {
        public PeerConnectionType ConnectionType { get; } = PeerConnectionType.RestApiV1;

        public IPeerClient Open(PeerModel peer) => new RESTPeerClient(this, peer);
    }
}