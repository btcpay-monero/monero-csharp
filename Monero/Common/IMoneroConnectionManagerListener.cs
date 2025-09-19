namespace Monero.Common;

public interface IMoneroConnectionManagerListener
{
    void OnConnectionChanged(MoneroRpcConnection? connection);
}