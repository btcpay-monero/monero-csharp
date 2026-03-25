using Monero.Daemon;
using Monero.IntegrationTests.Utils;
using Monero.Wallet;

namespace Monero.IntegrationTests;

public abstract class MoneroIntegrationTestBase
{
    protected static readonly MoneroDaemonRpc Daemon;

    protected static readonly MoneroWalletRpc Wallet;

    static MoneroIntegrationTestBase()
    {
        Daemon = TestUtils.GetDaemonRpc();
        Wallet = TestUtils.GetWalletRpc().Result;

        ulong daemonHeight = Daemon.GetHeight().GetAwaiter().GetResult();
        if (daemonHeight == 1)
        {
            Daemon.WaitForNextBlockHeader().GetAwaiter().GetResult();
        }
    }
}