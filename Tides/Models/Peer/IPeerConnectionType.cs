namespace Tides.Models.Peer
{
    public interface IPeerConnectionType
    {
        PeerConnectionType ConnectionType { get; }

        IPeerClient Open(PeerModel peer);
    }
}