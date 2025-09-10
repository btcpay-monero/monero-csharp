using Monero.Common;
using Monero.Test.Utils;
using Monero.Wallet;
using Monero.Wallet.Common;

namespace Monero.Test;

public class TestMoneroWalletRpc : TestMoneroWalletCommon
{
    protected override void CloseWallet(MoneroWallet wallet, bool save)
    {
        throw new NotImplementedException();
    }

    protected override MoneroWallet CreateWallet(MoneroWalletConfig config)
    {
        throw new NotImplementedException();
    }

    protected override List<string> GetSeedLanguages()
    {
        throw new NotImplementedException();
    }

    protected override MoneroWallet GetTestWallet()
    {
        return TestUtils.GetWalletRpc();
    }

    protected override MoneroWallet OpenWallet(MoneroWalletConfig config)
    {
        // assign defaults
        if (config == null)
        {
            config = new MoneroWalletConfig();
        }

        if (config.GetPassword() == null)
        {
            config.SetPassword(TestUtils.WALLET_PASSWORD);
        }

        if (config.GetServer() == null && config.GetConnectionManager() == null)
        {
            config.SetServer(daemon.GetRpcConnection());
        }

        // create client connected to internal monero-wallet-rpc process
        bool offline = TestUtils.OFFLINE_SERVER_URI.Equals(config.GetServerUri());
        MoneroWalletRpc wallet = TestUtils.StartWalletRpcProcess(offline);

        // open wallet
        try
        {
            wallet.OpenWallet(config);
            wallet.SetDaemonConnection(wallet.GetDaemonConnection(), true, null); // set daemon as trusted
            if (wallet.IsConnectedToDaemon())
            {
                wallet.StartSyncing((ulong)TestUtils.SYNC_PERIOD_IN_MS);
            }

            return wallet;
        }
        catch (MoneroError e)
        {
            try { TestUtils.StopWalletRpcProcess(wallet); }
            catch (Exception e2) { throw new Exception(e2.Message); }

            throw;
        }
    }
}