using Monero.Common;
using Monero.Wallet.Common;

namespace Monero.Wallet;

internal class MoneroWalletPoller
{
    private readonly TaskLooper _looper;
    private readonly object _lock = new();

    private readonly HashSet<string>
        _prevConfirmedNotifications = []; // tx hashes of previously confirmed but not yet unlocked notifications

    private readonly HashSet<string> _prevUnconfirmedNotifications = []; // tx hashes of previous notifications

    private readonly ulong _syncPeriodInMs; // default sync period in ms
    private readonly MoneroWalletDefault _wallet;
    private bool _isPolling;
    private int _numPolling;
    private List<ulong> _prevBalances = [];
    private ulong _prevHeight;
    private List<MoneroTxWallet> _prevLockedTxs = [];

    public MoneroWalletPoller(MoneroWalletDefault wallet, ulong syncPeriodInMs)
    {
        this._wallet = wallet;
        _looper = new TaskLooper(() => Poll());
        this._syncPeriodInMs = syncPeriodInMs;
    }

    // TODO: factor to common wallet rpc listener
    private void CheckForChangedBalances()
    {
        ulong balance = _wallet.GetBalance();
        ulong unlockedBalance = _wallet.GetUnlockedBalance();
        List<ulong> balances = [balance, unlockedBalance];
        if (balances[0] == _prevBalances[0] && balances[1] == _prevBalances[1])
        {
            return;
        }

        _prevBalances = balances;
        AnnounceBalancesChanged(balances[0], balances[1]);
    }

    public void SetIsPolling(bool isPolling)
    {
        this._isPolling = isPolling;
        if (isPolling)
        {
            _looper.Start(_syncPeriodInMs);
        }
        else
        {
            _looper.Stop();
        }
    }

    public void SetPeriodInMs(ulong periodInMs)
    {
        _looper.SetPeriodInMs(periodInMs);
    }

    public void Poll()
    {
        // skip if next poll is queued
        if (_numPolling > 1)
        {
            return;
        }

        _numPolling++;

        // synchronize polls
        lock (_lock)
        {
            try
            {
                // skip if wallet is closed
                if (_wallet.IsClosed())
                {
                    _numPolling--;
                    return;
                }

                // take initial snapshot
                if (_prevBalances.Count == 0)
                {
                    _prevHeight = _wallet.GetHeight();
                    _prevLockedTxs = _wallet.GetTxs(new MoneroTxQuery().SetIsLocked(true));
                    _prevBalances = GetBalances(null, null);
                    _numPolling--;
                    return;
                }

                // announce height changes
                ulong height = _wallet.GetHeight();
                if (_prevHeight != height)
                {
                    for (ulong i = _prevHeight; i < height; i++)
                    {
                        OnNewBlock(i);
                    }

                    _prevHeight = height;
                }

                // get locked txs for comparison to previous
                ulong minHeight = Math.Max(0, height - 70); // only monitor recent txs
                List<MoneroTxWallet> lockedTxs = _wallet.GetTxs(new MoneroTxQuery().SetIsLocked(true)
                    .SetMinHeight(minHeight).SetIncludeOutputs(true));

                // collect hashes of txs no longer locked
                List<string> noLongerLockedHashes = [];
                foreach (MoneroTxWallet prevLockedTx in _prevLockedTxs)
                {
                    if (GetTx(lockedTxs, prevLockedTx.GetHash()) == null)
                    {
                        noLongerLockedHashes.Add(prevLockedTx.GetHash()!);
                    }
                }

                // save locked txs for next comparison
                _prevLockedTxs = lockedTxs;

                // fetch txs which are no longer locked
                List<MoneroTxWallet> unlockedTxs = noLongerLockedHashes.Count == 0
                    ? []
                    : _wallet.GetTxs(new MoneroTxQuery().SetIsLocked(false).SetMinHeight(minHeight)
                        .SetHashes(noLongerLockedHashes).SetIncludeOutputs(true));

                // announce new unconfirmed and confirmed txs
                foreach (MoneroTxWallet lockedTx in lockedTxs)
                {
                    bool unannounced = lockedTx.IsConfirmed() == true
                        ? _prevConfirmedNotifications.Add(lockedTx.GetHash()!)
                        : _prevUnconfirmedNotifications.Add(lockedTx.GetHash()!);
                    if (unannounced)
                    {
                        NotifyOutputs(lockedTx);
                    }
                }

                // announce new unlocked outputs
                foreach (MoneroTxWallet unlockedTx in unlockedTxs)
                {
                    _prevUnconfirmedNotifications.Remove(unlockedTx.GetHash()!); // stop tracking tx notifications
                    _prevConfirmedNotifications.Remove(unlockedTx.GetHash()!);
                    NotifyOutputs(unlockedTx);
                }

                // announce balance changes
                CheckForChangedBalances();
                _numPolling--;
            }
            catch (Exception e)
            {
                _numPolling--;
                if (_isPolling)
                {
                    MoneroUtils.Log(0, "Failed to background poll wallet '" + _wallet.GetPath() + "': " + e.Message);
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
                .SetAmount(tx.GetOutgoingTransfer()!.GetAmount() + tx.GetFee())
                .SetAccountIndex(tx.GetOutgoingTransfer()!.GetAccountIndex())
                .SetSubaddressIndex(tx.GetOutgoingTransfer()!.GetSubaddressIndices()!.Count == 1
                    ? tx.GetOutgoingTransfer()!.GetSubaddressIndices()![0]
                    : null) // initialize if transfer sourced from single subaddress
                .SetTx(tx);
            tx.SetInputsWallet([output]);
            AnnounceOutputSpent(output);
        }

        // notify received outputs
        if (tx.GetIncomingTransfers() != null)
        {
            if (tx.GetOutputs() != null && tx.GetOutputs()!.Count > 0)
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
                foreach (MoneroIncomingTransfer transfer in tx.GetIncomingTransfers()!)
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

    private static MoneroTxWallet? GetTx(List<MoneroTxWallet> txs, string? txHash)
    {
        if (txHash == null)
        {
            throw new MoneroError("Tx hash is null");
        }

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
        return _wallet.GetListeners();
    }

    public List<ulong> GetBalances(uint? accountIdx, uint? subaddressIdx)
    {
        ulong balance = _wallet.GetBalance(accountIdx, subaddressIdx);
        ulong unlockedBalance = _wallet.GetUnlockedBalance(accountIdx, subaddressIdx);
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