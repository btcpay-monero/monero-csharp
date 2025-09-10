using Monero.Common;
using Monero.Wallet.Common;

namespace Monero.Wallet;

internal class MoneroWalletPoller
{
    private readonly TaskLooper looper;

    private readonly HashSet<string>
        prevConfirmedNotifications = []; // tx hashes of previously confirmed but not yet unlocked notifications

    private readonly HashSet<string> prevUnconfirmedNotifications = []; // tx hashes of previous notifications

    private readonly ulong syncPeriodInMs = 5000; // default sync period in ms
    private readonly MoneroWalletDefault wallet;
    private bool isPolling;
    private int numPolling;
    private List<ulong> prevBalances = [];
    private ulong prevHeight;
    private List<MoneroTxWallet> prevLockedTxs = [];

    public MoneroWalletPoller(MoneroWalletDefault wallet, ulong syncPeriodInMs)
    {
        this.wallet = wallet;
        looper = new TaskLooper(() => Poll());
        this.syncPeriodInMs = syncPeriodInMs;
    }

    // TODO: factor to common wallet rpc listener
    private bool CheckForChangedBalances()
    {
        ulong balance = wallet.GetBalance();
        ulong unlockedBalance = wallet.GetUnlockedBalance();
        List<ulong> balances = [balance, unlockedBalance];
        if (balances[0] != prevBalances[0] || balances[1] != prevBalances[1])
        {
            prevBalances = balances;
            AnnounceBalancesChanged(balances[0], balances[1]);
            return true;
        }

        return false;
    }

    public void SetIsPolling(bool isPolling)
    {
        this.isPolling = isPolling;
        if (isPolling)
        {
            looper.Start(syncPeriodInMs);
        }
        else
        {
            looper.Stop();
        }
    }

    public void SetPeriodInMs(ulong periodInMs)
    {
        looper.SetPeriodInMs(periodInMs);
    }

    public void Poll()
    {
        // skip if next poll is queued
        if (numPolling > 1)
        {
            return;
        }

        numPolling++;

        // synchronize polls
        lock (this)
        {
            try
            {
                // skip if wallet is closed
                if (wallet.IsClosed())
                {
                    numPolling--;
                    return;
                }

                // take initial snapshot
                if (prevBalances == null)
                {
                    prevHeight = wallet.GetHeight();
                    prevLockedTxs = wallet.GetTxs(new MoneroTxQuery().SetIsLocked(true));
                    prevBalances = GetBalances(null, null);
                    numPolling--;
                    return;
                }

                // announce height changes
                ulong height = wallet.GetHeight();
                if (prevHeight != height)
                {
                    for (ulong i = prevHeight; i < height; i++)
                    {
                        OnNewBlock(i);
                    }

                    prevHeight = height;
                }

                // get locked txs for comparison to previous
                ulong minHeight = Math.Max(0, height - 70); // only monitor recent txs
                List<MoneroTxWallet> lockedTxs = wallet.GetTxs(new MoneroTxQuery().SetIsLocked(true)
                    .SetMinHeight(minHeight).SetIncludeOutputs(true));

                // collect hashes of txs no longer locked
                List<string> noLongerLockedHashes = [];
                foreach (MoneroTxWallet prevLockedTx in prevLockedTxs)
                {
                    if (GetTx(lockedTxs, prevLockedTx.GetHash()) == null)
                    {
                        noLongerLockedHashes.Add(prevLockedTx.GetHash());
                    }
                }

                // save locked txs for next comparison
                prevLockedTxs = lockedTxs;

                // fetch txs which are no longer locked
                List<MoneroTxWallet> unlockedTxs = noLongerLockedHashes.Count == 0
                    ? []
                    : wallet.GetTxs(new MoneroTxQuery().SetIsLocked(false).SetMinHeight(minHeight)
                        .SetHashes(noLongerLockedHashes).SetIncludeOutputs(true));

                // announce new unconfirmed and confirmed txs
                foreach (MoneroTxWallet lockedTx in lockedTxs)
                {
                    bool unannounced = lockedTx.IsConfirmed() == true
                        ? prevConfirmedNotifications.Add(lockedTx.GetHash())
                        : prevUnconfirmedNotifications.Add(lockedTx.GetHash());
                    if (unannounced)
                    {
                        NotifyOutputs(lockedTx);
                    }
                }

                // announce new unlocked outputs
                foreach (MoneroTxWallet unlockedTx in unlockedTxs)
                {
                    prevUnconfirmedNotifications.Remove(unlockedTx.GetHash()); // stop tracking tx notifications
                    prevConfirmedNotifications.Remove(unlockedTx.GetHash());
                    NotifyOutputs(unlockedTx);
                }

                // announce balance changes
                CheckForChangedBalances();
                numPolling--;
            }
            catch (Exception e)
            {
                numPolling--;
                if (isPolling)
                {
                    MoneroUtils.Log(0, "Failed to background poll wallet '" + wallet.GetPath() + "': " + e.Message);
                }
            }
        }
    }

    private void NotifyOutputs(MoneroTxWallet tx)
    {
        // notify spent outputs // TODO (monero-project): monero-wallet-rpc does not allow scrape of tx inputs so providing one input with outgoing amount
        if (tx.GetOutgoingTransfer() != null)
        {
            if (tx.GetInputs() != null)
            {
                throw new MoneroError("Expected null inputs");
            }

            MoneroOutputWallet output = new MoneroOutputWallet()
                .SetAmount(tx.GetOutgoingTransfer().GetAmount() + tx.GetFee())
                .SetAccountIndex(tx.GetOutgoingTransfer().GetAccountIndex())
                .SetSubaddressIndex(tx.GetOutgoingTransfer().GetSubaddressIndices().Count == 1
                    ? tx.GetOutgoingTransfer().GetSubaddressIndices()[0]
                    : null) // initialize if transfer sourced from single subaddress
                .SetTx(tx);
            tx.SetInputsWallet([output]);
            AnnounceOutputSpent(output);
        }

        // notify received outputs
        if (tx.GetIncomingTransfers() != null)
        {
            if (tx.GetOutputs() != null && tx.GetOutputs().Count > 0)
            {
                // TODO (monero-project): outputs only returned for confirmed txs
                foreach (MoneroOutputWallet output in tx.GetOutputsWallet())
                {
                    AnnounceOutputReceived(output);
                }
            }
            else
            {
                // TODO (monero-project): monero-wallet-rpc does not allow scrape of unconfirmed received outputs so using incoming transfer values
                List<MoneroOutputWallet> outputs = [];
                foreach (MoneroIncomingTransfer transfer in tx.GetIncomingTransfers())
                {
                    outputs.Add(new MoneroOutputWallet()
                        .SetAccountIndex(transfer.GetAccountIndex())
                        .SetSubaddressIndex(transfer.GetSubaddressIndex())
                        .SetAmount(transfer.GetAmount())
                        .SetTx(tx));
                }

                tx.SetOutputsWallet(outputs);
                foreach (MoneroOutputWallet output in tx.GetOutputsWallet())
                {
                    AnnounceOutputReceived(output);
                }
            }
        }
    }

    private void OnNewBlock(ulong height)
    {
        AnnounceNewBlock(height);
    }

    private static MoneroTxWallet? GetTx(List<MoneroTxWallet> txs, string txHash)
    {
        foreach (MoneroTxWallet tx in txs)
        {
            if (txHash.Equals(tx.GetHash()))
            {
                return tx;
            }
        }

        return null;
    }

    private List<MoneroWalletListener> GetListeners()
    {
        return wallet.GetListeners();
    }

    public List<ulong> GetBalances(uint? accountIdx, uint? subaddressIdx)
    {
        ulong balance = wallet.GetBalance(accountIdx, subaddressIdx);
        ulong unlockedBalance = wallet.GetUnlockedBalance(accountIdx, subaddressIdx);
        return [balance, unlockedBalance];
    }

    protected void AnnounceSyncProgress(ulong height, ulong startHeight, ulong endHeight, double percentDone,
        string message)
    {
        foreach (MoneroWalletListener listener in GetListeners())
        {
            try
            {
                listener.OnSyncProgress(height, startHeight, endHeight, percentDone, message);
            }
            catch (Exception e)
            {
                MoneroUtils.Log(0, "Error calling listener on sync progress: " + e.Message);
            }
        }
    }

    protected void AnnounceNewBlock(ulong height)
    {
        foreach (MoneroWalletListener listener in GetListeners())
        {
            try
            {
                listener.OnNewBlock(height);
            }
            catch (Exception e)
            {
                MoneroUtils.Log(0, "Error calling listener on new block: " + e.Message);
            }
        }
    }

    protected void AnnounceBalancesChanged(ulong balance, ulong unlockedBalance)
    {
        foreach (MoneroWalletListener listener in GetListeners())
        {
            try
            {
                listener.OnBalancesChanged(balance, unlockedBalance);
            }
            catch (Exception e)
            {
                MoneroUtils.Log(0, "Error calling listener on balances changed: " + e.Message);
            }
        }
    }

    protected void AnnounceOutputReceived(MoneroOutputWallet output)
    {
        foreach (MoneroWalletListener listener in GetListeners())
        {
            try
            {
                listener.OnOutputReceived(output);
            }
            catch (Exception e)
            {
                MoneroUtils.Log(0, "Error calling listener on output received: " + e.Message);
            }
        }
    }

    protected void AnnounceOutputSpent(MoneroOutputWallet output)
    {
        foreach (MoneroWalletListener listener in GetListeners())
        {
            try
            {
                listener.OnOutputSpent(output);
            }
            catch (Exception e)
            {
                MoneroUtils.Log(0, "Error calling listener on output spent: " + e.Message);
            }
        }
    }
}