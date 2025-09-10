namespace Monero.Common;

public interface MoneroConnectionManagerListener
{
    void OnConnectionChanged(MoneroRpcConnection? connection);
}