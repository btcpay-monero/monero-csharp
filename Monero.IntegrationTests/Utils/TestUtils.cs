using System.Numerics;

using Monero.Common;
using Monero.Daemon;
using Monero.Wallet;
using Monero.Wallet.Common;

using Xunit;

namespace Monero.IntegrationTests.Utils;

internal abstract class TestUtils
{
    private static MoneroDaemonRpc? daemonRpc;
    private static MoneroWalletRpc? walletRpc;

    public static readonly bool TestsInContainer = GetDefaultEnv("TESTS_INCONTAINER", "false") == "true";

    public static readonly Uri DaemonRpcUri = new(GetDefaultEnv("XMR_DAEMON_URI", "http://127.0.0.1:18081"));
    public const string DaemonRpcUsername = "";
    public const string DaemonRpcPassword = "";

    public static readonly Uri PrimaryWalletRpcUri = new(GetDefaultEnv("XMR_WALLET_1_URI", "http://127.0.0.1:18082"));
    private static readonly Uri SecondaryWalletRpcUri = new(GetDefaultEnv("XMR_WALLET_2_URI", "http://127.0.0.1:18083"));
    public const string WalletRpcUsername = "";
    public const string WalletRpcPassword = "";

    // test wallet config
    private const string WalletName = "test_wallet_1";
    public const string WalletPassword = "supersecretpassword123";

    // test wallet constants
    public const MoneroNetworkType NetworkType = MoneroNetworkType.Mainnet;

    public const string Seed = "arena fossil anchor tapestry iguana tubes javelin gotten cafe damp talent angled onslaught haggled moon roles gills cigar cowl awning vapidly sighting buzzer delayed iguana";

    public const string Address = "4B7nn4hBQhaJ2MBWHLpdUHQMoMqgE2BWtZfofNxTDAJoGgckeEGm4f9WaBuFJmCKuwZ7FE3Di7biKbdafqE4JDj19MWPvQ9";

    public const ulong FirstReceiveHeight = 171; // NOTE: this value must be the height of the wallet's first tx for tests

    public const int SyncPeriodInMs = 5000; // period between wallet syncs in milliseconds

    public static MoneroDaemonRpc GetDaemonRpc()
    {
        if (daemonRpc != null)
        {
            return daemonRpc;
        }

        MoneroRpcConnection rpc = new(DaemonRpcUri, DaemonRpcUsername, DaemonRpcPassword);
        daemonRpc = new MoneroDaemonRpc(rpc);

        return daemonRpc;
    }

    public static Task<MoneroWalletRpc> GetCreateWallet()
    {
        MoneroRpcConnection connection = new(SecondaryWalletRpcUri, WalletRpcUsername, WalletRpcPassword);
        return Task.FromResult(new MoneroWalletRpc(connection));
    }

    public static async Task<MoneroWalletRpc> GetWalletRpc()
    {
        if (walletRpc == null)
        {
            MoneroRpcConnection rpc = new(PrimaryWalletRpcUri, WalletRpcUsername, WalletRpcPassword);
            walletRpc = new MoneroWalletRpc(rpc);
        }

        // attempt to open a test wallet
        try
        {
            await walletRpc.OpenWallet(WalletName, WalletPassword);
        }
        catch (JsonRpcApiException e)
        {
            // -1 returned when the wallet does not exist or fails to open e.g. it's already open by another application
            if (e.Error.Code == -1)
            {
                // create wallet
                await walletRpc.CreateWallet(new MoneroWalletConfig().SetPath(WalletName).SetPassword(WalletPassword)
                    .SetSeed(Seed).SetRestoreHeight(FirstReceiveHeight));
            }
            else
            {
                throw;
            }
        }

        // ensure we're testing the right wallet
        Assert.Equal(Seed, await walletRpc.GetSeed());
        Assert.Equal(Address, await walletRpc.GetPrimaryAddress());

        // sync and save wallet
        await walletRpc.Sync(null, null);
        await walletRpc.Save();
        await walletRpc.StartSyncing(SyncPeriodInMs);

        // return cached wallet rpc
        return walletRpc;
    }

    public static void TestUnsignedBigInteger(BigInteger? num, bool? nonZero = null)
    {
        if (num == null)
        {
            throw new ArgumentNullException(nameof(num), "BigInteger cannot be null");
        }

        int comparison = BigInteger.Compare((BigInteger)num, BigInteger.Zero);

        if (comparison < 0)
        {
            throw new MoneroError("BigInteger must be >= 0 (unsigned)");
        }

        if (nonZero == true && comparison <= 0)
        {
            throw new MoneroError("BigInteger must be > 0");
        }

        if (nonZero == false && comparison != 0)
        {
            throw new MoneroError("BigInteger must be == 0");
        }
    }

    private static string GetDefaultEnv(string key, string defaultValue)
    {
        string? currentValue = Environment.GetEnvironmentVariable(key);
        return string.IsNullOrEmpty(currentValue) ? defaultValue : currentValue;
    }
}