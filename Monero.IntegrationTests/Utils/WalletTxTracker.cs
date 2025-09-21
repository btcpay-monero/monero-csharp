using Monero.Common;
using Monero.Daemon;
using Monero.Daemon.Common;
using Monero.Wallet;
using Monero.Wallet.Common;

namespace Monero.IntegrationTests.Utils;

public class WalletTxTracker
{
    private readonly List<IMoneroWallet> clearedWallets = [];

    public void Reset()
    {
        clearedWallets.Clear();
    }

    public async Task WaitForWalletTxsToClearPool(IMoneroWallet wallet)
    {
        List<IMoneroWallet> wallets = [wallet];
        // get wallet tx hashes
        List<string> txHashesWallet = [];
        foreach (IMoneroWallet moneroWallet in wallets)
        {
            if (!clearedWallets.Contains(moneroWallet))
            {
                await moneroWallet.Sync();
                foreach (MoneroTxWallet tx in await moneroWallet.GetTxs())
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
        IMoneroDaemon daemon = TestUtils.GetDaemonRpc();
        while (true)
        {
            // get hashes of relayed, non-failed txs in the pool
            List<string> txHashesPool = [];
            foreach (MoneroTx tx in await daemon.GetTxPool())
            {
                if (tx.IsRelayed() != true)
                {
                    continue;
                }

                if (tx.IsFailed() == true)
                {
                    await daemon.FlushTxPool([tx.GetHash()!]); // flush tx if failed
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
                MoneroMiningStatus miningStatus = await daemon.GetMiningStatus();
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
            await daemon.StopMining();
        }

        // sync wallets with the pool
        foreach (IMoneroWallet moneroWallet in wallets)
        {
            await moneroWallet.Sync();
            clearedWallets.Add(moneroWallet);
        }
    }
}