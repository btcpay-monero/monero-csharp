using Monero.Wallet.Common;

namespace Monero.Wallet;

public class MoneroWalletListener
{
    public virtual void OnSyncProgress(ulong height, ulong startHeight, ulong endHeight, double percentDone,
        string message)
    {
    }

    public virtual void OnNewBlock(ulong height) { }
    public virtual void OnBalancesChanged(ulong newBalance, ulong newUnlockedBalance) { }
    public virtual void OnOutputReceived(MoneroOutputWallet output) { }
    public virtual void OnOutputSpent(MoneroOutputWallet output) { }
}