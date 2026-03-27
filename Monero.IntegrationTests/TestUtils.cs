using System.Numerics;

using Monero.Common;
using Monero.Daemon;
using Monero.Wallet;

namespace Monero.IntegrationTests;

internal abstract class TestUtils
{
    private static MoneroDaemonRpc? daemonRpc;
    private static MoneroWalletRpc? walletRpc;

    public static readonly bool TestsInContainer = GetDefaultEnv("TESTS_INCONTAINER", "false") == "true";

    public static readonly Uri DaemonRpcUri = new(GetDefaultEnv("XMR_DAEMON_URI", "http://127.0.0.1:28081"));
    public const string DaemonRpcUsername = "";
    public const string DaemonRpcPassword = "";

    public static readonly Uri PrimaryWalletRpcUri = new(GetDefaultEnv("XMR_WALLET_1_URI", "http://127.0.0.1:18082"));
    private static readonly Uri SecondaryWalletRpcUri = new(GetDefaultEnv("XMR_WALLET_2_URI", "http://127.0.0.1:18083"));
    public const string WalletRpcUsername = "";
    public const string WalletRpcPassword = "";

    // test wallet config
    public const string WalletName = "test_wallet_1";
    public const string WalletPassword = "supersecretpassword123";

    public const string Seed = "origin hickory pavements tudor sizes hornet tether segments sack technical elbow unsafe legion nitrogen adapt yearbook idols fuzzy pitched goes tusks elbow erase fossil erase";

    public const string Address = "9xSyMy1r9h3BVjMrF3CTqQCQy36yCfkpn7uVfMyTUbez3hhumqBUqGUNNALjcd7f1HJBRdeH82bCC3veFHW7z3xm28gug4d";

    public const uint FirstReceiveHeight = 171; // NOTE: this value must be the height of the wallet's first tx for tests

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

    public static MoneroWalletRpc GetWalletRpc()
    {
        if (walletRpc != null)
        {
            return walletRpc;
        }

        MoneroRpcConnection rpc = new(PrimaryWalletRpcUri, WalletRpcUsername, WalletRpcPassword);
        walletRpc = new MoneroWalletRpc(rpc);

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