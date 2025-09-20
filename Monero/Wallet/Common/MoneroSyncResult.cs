namespace Monero.Wallet.Common;

public class MoneroSyncResult
{
    private ulong? _numBlocksFetched;
    private bool? _receivedMoney;

    public MoneroSyncResult()
    {
    }

    public MoneroSyncResult(ulong? numBlocksFetched, bool? receivedMoney)
    {
        _numBlocksFetched = numBlocksFetched;
        _receivedMoney = receivedMoney;
    }

    public MoneroSyncResult(MoneroSyncResult syncResult)
    {
        _numBlocksFetched = syncResult._numBlocksFetched;
        _receivedMoney = syncResult._receivedMoney;
    }

    public ulong? GetNumBlocksFetched()
    {
        return _numBlocksFetched;
    }

    public MoneroSyncResult SetNumBlocksFetched(ulong? numBlocksFetched)
    {
        _numBlocksFetched = numBlocksFetched;
        return this;
    }

    public bool? GetReceivedMoney()
    {
        return _receivedMoney;
    }

    public MoneroSyncResult SetReceivedMoney(bool? receivedMoney)
    {
        _receivedMoney = receivedMoney;
        return this;
    }

    public MoneroSyncResult Clone()
    {
        return new MoneroSyncResult(this);
    }
}