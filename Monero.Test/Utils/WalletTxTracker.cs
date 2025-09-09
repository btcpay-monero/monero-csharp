using Monero.Common;
using Monero.Daemon;
using Monero.Daemon.Common;
using Monero.Wallet;
using Monero.Wallet.Common;

namespace Monero.Test.Utils;

public class WalletTxTracker
{
    private readonly List<MoneroWallet> clearedWallets = [];

    public void Reset()
    {
        clearedWallets.Clear();
    }

    public void WaitForWalletTxsToClearPool(MoneroWallet wallet)
    {
        WaitForWalletTxsToClearPool([wallet]);
    }

    public void WaitForWalletTxsToClearPool(List<MoneroWallet> wallets)
    {

        // get wallet tx hashes
        List<string> txHashesWallet = [];
        foreach (MoneroWallet wallet in wallets)
        {
            if (!clearedWallets.Contains(wallet))
            {
                wallet.Sync();
                foreach (MoneroTxWallet tx in wallet.GetTxs())
                {
                    string? txHash = tx.GetHash();
                    if (txHash == null) continue;  // skip txs without hashes
                    txHashesWallet.Add(txHash);
                }
            }
        }

        // loop until all wallet txs clear from pool
        bool isFirst = true;
        bool miningStarted = false;
        MoneroDaemon daemon = TestUtils.GetDaemonRpc();
        while (true)
        {

            // get hashes of relayed, non-failed txs in the pool
            List<string> txHashesPool = [];
            foreach (MoneroTx tx in daemon.GetTxPool())
            {
                if (tx.IsRelayed() != true) continue;
                else if (tx.IsFailed() == true) daemon.FlushTxPool(tx.GetHash());  // flush tx if failed
                else txHashesPool.Add(tx.GetHash());
            }

            // get hashes to wait for as intersection of wallet and pool txs
            txHashesPool = txHashesPool.Intersect(txHashesWallet).ToList();

            // break if no txs to wait for
            if (txHashesPool.Count == 0) break;

            // if first time waiting, log message and start mining
            if (isFirst)
            {
                isFirst = false;
                MoneroUtils.Log(0, "Waiting for wallet txs to clear from the pool in order to fully sync and avoid double spend attempts (known issue)");
                MoneroMiningStatus miningStatus = daemon.GetMiningStatus();
                if (!miningStatus.IsActive() == true)
                {
                    try
                    {
                        StartMining.Start();
                        miningStarted = true;
                    }
                    catch (Exception e) { } // no problem
                }
            }

            // sleep for a moment
            try { Thread.Sleep(TestUtils.SYNC_PERIOD_IN_MS); }
            catch (Exception e) { throw new MoneroError(e.Message); }
        }

        // stop mining if started mining
        if (miningStarted) daemon.StopMining();

        // sync wallets with the pool
        foreach (MoneroWallet wallet in wallets)
        {
            wallet.Sync();
            clearedWallets.Add(wallet);
        }
    }

}