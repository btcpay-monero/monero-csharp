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

    // directory with monero binaries to test (monerod and monero-wallet-rpc)
    public static readonly string MONERO_BINS_DIR = "";

    // monero daemon rpc endpoint configuration (change per your configuration)
    public static readonly bool TESTS_INCONTAINER = GetDefaultEnv("TESTS_INCONTAINER", "false") == "true";
    public static readonly string DAEMON_RPC_URI = GetDefaultEnv("XMR_DAEMON_URI", "http://127.0.0.1:18081");
    public static readonly string DAEMON_RPC_USERNAME = "";
    public static readonly string DAEMON_RPC_PASSWORD = "";
    public static readonly string DAEMON_LOCAL_PATH = MONERO_BINS_DIR + "/monerod";

    public static readonly bool WALLET_RPC_ZMQ_ENABLED = false;
    public static readonly int WALLET_RPC_ZMQ_PORT_START = 58083;
    public static readonly int WALLET_RPC_ZMQ_BIND_PORT_START = 48083; // TODO: zmq bind port necessary?
    public static readonly string WALLET_RPC_USERNAME = "";
    public static readonly string WALLET_RPC_PASSWORD = "";
    public static readonly string WALLET_RPC_ZMQ_DOMAIN = "127.0.0.1";
    public static readonly string WALLET_RPC_URI = GetDefaultEnv("XMR_WALLET_1_URI", "http://127.0.0.1:18082");
    public static readonly string CREATE_WALLET_RPC_URI = GetDefaultEnv("XMR_WALLET_2_URI", "http://127.0.0.1:18083");

    public static readonly string WALLET_RPC_ZMQ_URI =
        "tcp://" + WALLET_RPC_ZMQ_DOMAIN + ":" + WALLET_RPC_ZMQ_PORT_START;

    public static readonly string WALLET_RPC_LOCAL_PATH = MONERO_BINS_DIR + "/monero-wallet-rpc";
    public static readonly string WALLET_RPC_LOCAL_WALLET_DIR = MONERO_BINS_DIR;

    public static readonly string
        WALLET_RPC_ACCESS_CONTROL_ORIGINS = "http://localhost:8080"; // cors accesses from a web browser

    // test wallet config
    public static readonly string WALLET_NAME = "test_wallet_1";
    public static readonly string WALLET_PASSWORD = "supersecretpassword123";
    public static readonly string TEST_WALLETS_DIR = "./test_wallets";
    public static readonly string WALLET_FULL_PATH = TEST_WALLETS_DIR + "/" + WALLET_NAME;

    // test wallet constants
    public static readonly ulong MAX_FEE = 75000000000;
    public static readonly MoneroNetworkType NETWORK_TYPE = MoneroNetworkType.Mainnet;
    public static readonly string LANGUAGE = "English";

    public static readonly string SEED =
        "arena fossil anchor tapestry iguana tubes javelin gotten cafe damp talent angled onslaught haggled moon roles gills cigar cowl awning vapidly sighting buzzer delayed iguana";

    public static readonly string ADDRESS =
        "4B7nn4hBQhaJ2MBWHLpdUHQMoMqgE2BWtZfofNxTDAJoGgckeEGm4f9WaBuFJmCKuwZ7FE3Di7biKbdafqE4JDj19MWPvQ9";

    public static readonly string PRIVATE_VIEW_KEY = "395d05e724f4c08f072895eab08ee4d00b3b2848902cf939fd3c07288454f804";

    public static readonly ulong
        FIRST_RECEIVE_HEIGHT = 171; // NOTE: this value must be the height of the wallet's first tx for tests

    public static readonly int SYNC_PERIOD_IN_MS = 5000; // period between wallet syncs in milliseconds

    public static readonly int AUTO_CONNECT_TIMEOUT_MS = 3000;

    public static readonly WalletTxTracker WALLET_TX_TRACKER = new();

    public static MoneroWalletRpc StartWalletRpcProcess(bool offline = false)
    {
        throw new NotImplementedException();
    }

    public static void StopWalletRpcProcess(MoneroWalletRpc wallet)
    {
        throw new NotImplementedException();
    }

    public static MoneroDaemonRpc GetDaemonRpc()
    {
        if (daemonRpc == null)
        {
            MoneroRpcConnection rpc = new(DAEMON_RPC_URI, DAEMON_RPC_USERNAME, DAEMON_RPC_PASSWORD);
            daemonRpc = new MoneroDaemonRpc(rpc);
        }

        return daemonRpc;
    }

    public static async Task<MoneroWalletRpc> GetCreateWallet()
    {
        MoneroRpcConnection connection = new(CREATE_WALLET_RPC_URI);
        await connection.CheckConnection();
        return new MoneroWalletRpc(connection);
    }

    public static async Task<MoneroWalletRpc> GetWalletRpc()
    {
        if (walletRpc == null)
        {
            MoneroRpcConnection rpc = new(WALLET_RPC_URI, WALLET_RPC_USERNAME, WALLET_RPC_PASSWORD,
                WALLET_RPC_ZMQ_URI, 2);
            walletRpc = new MoneroWalletRpc(rpc);
        }

        // attempt to open a test wallet
        try
        {
            await walletRpc.OpenWallet(WALLET_NAME, WALLET_PASSWORD);
        }
        catch (MoneroRpcError e)
        {
            // -1 returned when the wallet does not exist or fails to open e.g. it's already open by another application
            if (e.GetCode() == -1)
            {
                // create wallet
                await walletRpc.CreateWallet(new MoneroWalletConfig().SetPath(WALLET_NAME).SetPassword(WALLET_PASSWORD)
                    .SetSeed(SEED).SetRestoreHeight(FIRST_RECEIVE_HEIGHT));
            }
            else
            {
                throw;
            }
        }

        // ensure we're testing the right wallet
        Assert.Equal(SEED, await walletRpc.GetSeed());
        Assert.Equal(ADDRESS, await walletRpc.GetPrimaryAddress());

        // sync and save wallet
        await walletRpc.Sync(null, null);
        await walletRpc.Save();
        await walletRpc.StartSyncing((ulong)SYNC_PERIOD_IN_MS);

        // return cached wallet rpc
        return walletRpc;
    }

    public static MoneroWalletRpc GetWalletRpcSync()
    {
        return GetWalletRpc().GetAwaiter().GetResult();
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

    public static string GetMiningAddress()
    {
        if (NETWORK_TYPE == MoneroNetworkType.Mainnet)
        {
            return "42U9v3qs5CjZEePHBZHwuSckQXebuZu299NSmVEmQ41YJZQhKcPyujyMSzpDH4VMMVSBo3U3b54JaNvQLwAjqDhKS3rvM3L";
        }

        if (NETWORK_TYPE == MoneroNetworkType.Testnet)
        {
            return "9tUBnNCkC3UKGygHCwYvAB1FscpjUuq5e9MYJd2rXuiiTjjfVeSVjnbSG5VTnJgBgy9Y7GTLfxpZNMUwNZjGfdFr1z79eV1";
        }

        return "5B8s3obCY2ETeQB3GNAGPK2zRGen5UeW1WzegSizVsmf6z5NvM2GLoN6zzk1vHyzGAAfA8pGhuYAeCFZjHAp59jRVQkunGS";
    }

    private static string GetDefaultEnv(string key, string defaultValue)
    {
        string? currentValue = Environment.GetEnvironmentVariable(key);
        return string.IsNullOrEmpty(currentValue) ? defaultValue : currentValue;
    }
}