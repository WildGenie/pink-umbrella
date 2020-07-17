namespace Poncho.Models.Peer
{
    public interface IPeerConnectionType
    {
        PeerConnectionType ConnectionType { get; }

        IPeerClient Open(PeerModel peer);
    }
}