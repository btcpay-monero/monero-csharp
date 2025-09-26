using Monero.Common;
using Monero.Wallet.Common;

namespace Monero.Wallet;

internal class MoneroWalletPoller
{
    private readonly TaskLooper _looper;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private readonly ulong _syncPeriodInMs; // default sync period in ms
    private readonly MoneroWalletRpc _wallet;
    private bool _isPolling;
    private int _numPolling;
    private List<ulong> _prevBalances = [];
    private ulong _prevHeight;

    public MoneroWalletPoller(MoneroWalletRpc wallet, ulong syncPeriodInMs)
    {
        _wallet = wallet;
        _looper = new TaskLooper(Poll);
        _syncPeriodInMs = syncPeriodInMs;
    }

    // TODO: factor to common wallet rpc listener
    private async Task CheckForChangedBalances()
    {
        ulong balance = await _wallet.GetBalance(null, null);
        ulong unlockedBalance = await _wallet.GetUnlockedBalance(null, null);
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
        _isPolling = isPolling;
        if (isPolling)
        {
            _looper.Start(_syncPeriodInMs);
        }
        else
        {
            _looper.Stop();
        }
    }

    public bool IsPolling()
    {
        return _isPolling;
    }

    public void SetPeriodInMs(ulong periodInMs)
    {
        _looper.SetPeriodInMs(periodInMs);
    }

    public async Task Poll()
    {
        // skip if the next poll is queued
        if (_numPolling > 1)
        {
            return;
        }

        _numPolling++;

        // synchronize polls
        await _semaphore.WaitAsync();
        try
        {
            // skip if wallet is closed
            if (await _wallet.IsClosed())
            {
                _numPolling--;
                return;
            }

            // take initial snapshot
            if (_prevBalances.Count == 0)
            {
                _prevHeight = await _wallet.GetHeight();
                _prevBalances = await GetBalances(null, null);
                _numPolling--;
                return;
            }

            // announce height changes
            ulong height = await _wallet.GetHeight();
            if (_prevHeight != height)
            {
                for (ulong i = _prevHeight; i < height; i++)
                {
                    OnNewBlock(i);
                }

                _prevHeight = height;
            }

            // announce balance changes
            await CheckForChangedBalances();
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

        _semaphore.Release();
    }

    private void OnNewBlock(ulong height)
    {
        AnnounceNewBlock(height);
    }

    private List<MoneroWalletListener> GetListeners()
    {
        return _wallet.GetListeners();
    }

    public async Task<List<ulong>> GetBalances(uint? accountIdx, uint? subaddressIdx)
    {
        ulong balance = await _wallet.GetBalance(accountIdx, subaddressIdx);
        ulong unlockedBalance = await _wallet.GetUnlockedBalance(accountIdx, subaddressIdx);
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