using Monero.Common;
using Monero.Daemon;
using Monero.Daemon.Common;
using Monero.Wallet;
using Monero.Wallet.Common;

namespace Monero.IntegrationTests.Utils;

public class WalletTxTracker
{
    private readonly List<MoneroWallet> clearedWallets = [];

    public void Reset()
    {
        clearedWallets.Clear();
    }

    public void WaitForWalletTxsToClearPool(MoneroWallet wallet)
    {
        List<MoneroWallet> wallets = [wallet];
        // get wallet tx hashes
        List<string> txHashesWallet = [];
        foreach (MoneroWallet moneroWallet in wallets)
        {
            if (!clearedWallets.Contains(moneroWallet))
            {
                moneroWallet.Sync();
                foreach (MoneroTxWallet tx in moneroWallet.GetTxs())
                {
                    string? txHash = tx.GetHash();
                    if (txHash == null)
                    {
                        continue; // skip txs without hashes
                    }

                    txHashesWallet.Add(txHash);
                }
            }
        }

        // loop until all wallet txs clear from the pool
        bool isFirst = true;
        bool miningStarted = false;
        MoneroDaemon daemon = TestUtils.GetDaemonRpc();
        while (true)
        {
            // get hashes of relayed, non-failed txs in the pool
            List<string> txHashesPool = [];
            foreach (MoneroTx tx in daemon.GetTxPool())
            {
                if (tx.IsRelayed() != true)
                {
                    continue;
                }

                if (tx.IsFailed() == true)
                {
                    daemon.FlushTxPool(tx.GetHash()!); // flush tx if failed
                }
                else
                {
                    txHashesPool.Add(tx.GetHash()!);
                }
            }

            // get hashes to wait for as intersection of wallet and pool txs
            txHashesPool = txHashesPool.Intersect(txHashesWallet).ToList();

            // break if no txs to wait for
            if (txHashesPool.Count == 0)
            {
                break;
            }

            // if first time waiting, log a message and start mining
            if (isFirst)
            {
                isFirst = false;
                MoneroUtils.Log(0,
                    "Waiting for wallet txs to clear from the pool in order to fully sync and avoid double spend attempts (known issue)");
                MoneroMiningStatus miningStatus = daemon.GetMiningStatus();
                if (!miningStatus.IsActive() == true)
                {
                    try
                    {
                        StartMining.Start();
                        miningStarted = true;
                    }
                    catch (Exception)
                    {
                        // no problem
                    }
                }
            }

            // sleep for a moment
            try { Thread.Sleep(TestUtils.SYNC_PERIOD_IN_MS); }
            catch (Exception e) { throw new MoneroError(e.Message); }
        }

        // stop mining if started mining
        if (miningStarted)
        {
            daemon.StopMining();
        }

        // sync wallets with the pool
        foreach (MoneroWallet moneroWallet in wallets)
        {
            moneroWallet.Sync();
            clearedWallets.Add(moneroWallet);
        }
    }
}