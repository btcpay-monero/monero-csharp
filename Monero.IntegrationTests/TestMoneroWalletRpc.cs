using Monero.Common;
using Monero.IntegrationTests.Utils;
using Monero.Wallet;
using Monero.Wallet.Common;

namespace Monero.IntegrationTests;

public class TestMoneroWalletRpc : TestMoneroWalletCommon
{
    protected override Task CloseWallet(MoneroWallet walletInstance, bool save)
    {
        throw new NotImplementedException();
    }

    protected override Task<MoneroWallet> CreateWallet(MoneroWalletConfig config)
    {
        throw new NotImplementedException();
    }

    protected override Task<List<string>> GetSeedLanguages()
    {
        throw new NotImplementedException();
    }

    protected override async Task<MoneroWallet> GetTestWallet()
    {
        return await TestUtils.GetWalletRpc();
    }

    protected override async Task<MoneroWallet> OpenWallet(MoneroWalletConfig config)
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
            await moneroWalletRpc.OpenWallet(config);
            await moneroWalletRpc.SetDaemonConnection(await moneroWalletRpc.GetDaemonConnection(), true, null); // set daemon as trusted
            if (await moneroWalletRpc.IsConnectedToDaemon())
            {
                await moneroWalletRpc.StartSyncing((ulong)TestUtils.SYNC_PERIOD_IN_MS);
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