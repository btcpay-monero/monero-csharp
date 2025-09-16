using Monero.Common;
using Monero.IntegrationTests.Utils;
using Monero.Wallet;
using Monero.Wallet.Common;

namespace Monero.IntegrationTests;

public class TestMoneroWalletRpc : TestMoneroWalletCommon
{
    protected override void CloseWallet(MoneroWallet walletInstance, bool save)
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

        // create a client connected to an internal monero-wallet-rpc process
        bool offline = TestUtils.OFFLINE_SERVER_URI.Equals(config.GetServerUri());
        MoneroWalletRpc moneroWalletRpc = TestUtils.StartWalletRpcProcess(offline);

        // open wallet
        try
        {
            moneroWalletRpc.OpenWallet(config);
            moneroWalletRpc.SetDaemonConnection(moneroWalletRpc.GetDaemonConnection(), true, null); // set daemon as trusted
            if (moneroWalletRpc.IsConnectedToDaemon())
            {
                moneroWalletRpc.StartSyncing((ulong)TestUtils.SYNC_PERIOD_IN_MS);
            }

            return moneroWalletRpc;
        }
        catch (MoneroError)
        {
            try { TestUtils.StopWalletRpcProcess(moneroWalletRpc); }
            catch (Exception e2) { throw new Exception(e2.Message); }

            throw;
        }
    }
}