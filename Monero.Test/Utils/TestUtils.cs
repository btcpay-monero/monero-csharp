using Monero.Common;
using Monero.Daemon;
using Monero.Wallet;
using Monero.Wallet.Common;
using System.Data;
using System.Diagnostics;
using System.Numerics;

namespace Monero.Test.Utils
{
    internal abstract class TestUtils
    {
        private static MoneroDaemonRpc? daemonRpc = null;
        private static MoneroWalletRpc? walletRpc = null;

        // directory with monero binaries to test (monerod and monero-wallet-rpc)
        public static readonly string MONERO_BINS_DIR = "C:\\Users\\J.Argenio\\Documents\\monero-x86_64-w64-mingw32-v0.18.4.2";
  
        // monero daemon rpc endpoint configuration (change per your configuration)
        public static readonly string DAEMON_RPC_URI = "http://127.0.0.1:28081";
        public static readonly string DAEMON_RPC_USERNAME = "";
        public static readonly string DAEMON_RPC_PASSWORD = "";
        public static readonly string DAEMON_LOCAL_PATH = MONERO_BINS_DIR + "/monerod.exe";
  
        // monero wallet rpc configuration (change per your configuration)
        public static readonly int WALLET_RPC_PORT_START = 28084; // test wallet executables will bind to consecutive ports after these
        public static readonly bool WALLET_RPC_ZMQ_ENABLED = false;
        public static readonly int WALLET_RPC_ZMQ_PORT_START = 58083;
        public static readonly int WALLET_RPC_ZMQ_BIND_PORT_START = 48083;  // TODO: zmq bind port necessary?
        public static readonly string WALLET_RPC_USERNAME = "rpc_user";
        public static readonly string WALLET_RPC_PASSWORD = "abc123";
        public static readonly string WALLET_RPC_ZMQ_DOMAIN = "127.0.0.1";
        public static readonly string WALLET_RPC_DOMAIN = "localhost";
        public static readonly string WALLET_RPC_URI = WALLET_RPC_DOMAIN + ":" + WALLET_RPC_PORT_START;
        public static readonly string WALLET_RPC_ZMQ_URI = "tcp://" + WALLET_RPC_ZMQ_DOMAIN + ":" + WALLET_RPC_ZMQ_PORT_START;
        public static readonly string WALLET_RPC_LOCAL_PATH = MONERO_BINS_DIR + "/monero-wallet-rpc.exe";
        public static readonly string WALLET_RPC_LOCAL_WALLET_DIR = MONERO_BINS_DIR;
        public static readonly string WALLET_RPC_ACCESS_CONTROL_ORIGINS = "http://localhost:8080"; // cors access from web browser
  
        // test wallet config
        public static readonly string WALLET_NAME = "test_wallet_1";
        public static readonly string WALLET_PASSWORD = "supersecretpassword123";
        public static readonly string TEST_WALLETS_DIR = "./test_wallets";
        public static readonly string WALLET_FULL_PATH = TEST_WALLETS_DIR + "/" + WALLET_NAME;
  
        // test wallet constants
        public static readonly ulong MAX_FEE = 75000000000;
        public static readonly MoneroNetworkType NETWORK_TYPE = MoneroNetworkType.TESTNET;
        public static readonly string LANGUAGE = "English";
        public static readonly string SEED = "silk mocked cucumber lettuce hope adrenalin aching lush roles fuel revamp baptism wrist ulong tender teardrop midst pastry pigment equip frying inbound pinched ravine frying";
        public static readonly string ADDRESS = "A1y9sbVt8nqhZAVm3me1U18rUVXcjeNKuBd1oE2cTs8biA9cozPMeyYLhe77nPv12JA3ejJN3qprmREriit2fi6tJDi99RR";
        public static readonly ulong FIRST_RECEIVE_HEIGHT = 171; // NOTE: this value must be the height of the wallet's first tx for tests
        public static readonly int SYNC_PERIOD_IN_MS = 5000; // period between wallet syncs in milliseconds
        public static readonly string OFFLINE_SERVER_URI = "offline_server_uri"; // dummy server uri to remain offline because wallet2 connects to default if not given
        public static readonly int AUTO_CONNECT_TIMEOUT_MS = 3000;

        public static readonly WalletTxTracker WALLET_TX_TRACKER = new();

        public static Dictionary<MoneroWalletRpc, int> WALLET_PORT_OFFSETS = [];

        public static MoneroWalletRpc StartWalletRpcProcess(bool offline = false)
        {
            int portOffset = 1;
            while (WALLET_PORT_OFFSETS.ContainsValue(portOffset)) portOffset++;

            var cmd = new List<string>
            {
                WALLET_RPC_LOCAL_PATH,
                "--" + NETWORK_TYPE.ToString().ToLower(),
                "--rpc-bind-port", (WALLET_RPC_PORT_START + portOffset).ToString(),
                "--rpc-login", WALLET_RPC_USERNAME + ":" + WALLET_RPC_PASSWORD,
                "--wallet-dir", WALLET_RPC_LOCAL_WALLET_DIR,
                "--rpc-access-control-origins", WALLET_RPC_ACCESS_CONTROL_ORIGINS
            };

            if (offline)
            {
                cmd.Add("--offline");
            }
            else
            {
                cmd.Add("--daemon-address");
                cmd.Add(DAEMON_RPC_URI);
            }

            if (!string.IsNullOrEmpty(DAEMON_RPC_USERNAME))
            {
                cmd.Add("--daemon-login");
                cmd.Add(DAEMON_RPC_USERNAME + ":" + DAEMON_RPC_PASSWORD);
            }

            // ZMQ
            if (WALLET_RPC_ZMQ_ENABLED)
            {
                cmd.Add("--zmq-rpc-bind-port");
                cmd.Add((WALLET_RPC_ZMQ_BIND_PORT_START + portOffset).ToString());

                cmd.Add("--zmq-pub");
                cmd.Add($"tcp://{WALLET_RPC_ZMQ_DOMAIN}:{WALLET_RPC_ZMQ_PORT_START + portOffset}");
            }
            else
            {
                //TODO: enable this when zmq supported in monero - wallet - rpc
                // cmd.Add("--no-zmq");
            }

            try
            {
                var wallet = new MoneroWalletRpc(cmd);

                WALLET_PORT_OFFSETS[wallet] = portOffset;

                return wallet;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to start Monero wallet RPC process", e);
            }
        }

        public static void StopWalletRpcProcess(MoneroWalletRpc wallet)
        {
            WALLET_PORT_OFFSETS.Remove(wallet);
            wallet.StopProcess();
        }

        public static MoneroDaemonRpc GetDaemonRpc()
        {
            if (daemonRpc == null)
            {
                var rpc = new MoneroRpcConnection(DAEMON_RPC_URI, DAEMON_RPC_USERNAME, DAEMON_RPC_PASSWORD);
                daemonRpc = new MoneroDaemonRpc(rpc);
            }

            return daemonRpc;
        }

        public static MoneroWalletRpc GetWalletRpc()
        {
            if (walletRpc == null)
            {
                var rpc = new MoneroRpcConnection(WALLET_RPC_URI, WALLET_RPC_USERNAME, WALLET_RPC_PASSWORD, WALLET_RPC_ZMQ_URI, 2);
                walletRpc = new MoneroWalletRpc(rpc);
            }

            // attempt to open test wallet
            try
            {
                walletRpc.OpenWallet(WALLET_NAME, WALLET_PASSWORD);
            }
            catch (MoneroRpcError e)
            {
                // -1 returned when wallet does not exist or fails to open e.g. it's already open by another application
                if (e.GetCode() == -1)
                {
                    // create wallet
                    walletRpc.CreateWallet(new MoneroWalletConfig().SetPath(WALLET_NAME).SetPassword(WALLET_PASSWORD).SetSeed(SEED).SetRestoreHeight(FIRST_RECEIVE_HEIGHT));
                }
                else
                {
                    throw e;
                }
            }

            // ensure we're testing the right wallet
            Assert.Equal(SEED, walletRpc.GetSeed());
            Assert.Equal(ADDRESS, walletRpc.GetPrimaryAddress());

            // sync and save wallet
            walletRpc.Sync();
            walletRpc.Save();
            walletRpc.StartSyncing((ulong)SYNC_PERIOD_IN_MS);

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

            if (comparison < 0) throw new MoneroError("BigInteger must be >= 0 (unsigned)");

            if (nonZero == true && comparison <= 0)
            {
                throw new MoneroError("BigInteger must be > 0");
            }
            if (nonZero == false && comparison != 0)
            {
                throw new MoneroError("BigInteger must be == 0");
            }
        }

        public static string GetExternalWalletAddress()
        {
            var info = GetDaemonRpc().GetInfo();

            if (info == null)
            {
                throw new MoneroError("Failed to get daemon info");
            }

            MoneroNetworkType? networkType = info.GetNetworkType();
            
            switch (networkType)
            {
                case MoneroNetworkType.STAGENET:
                    return "78Zq71rS1qK4CnGt8utvMdWhVNMJexGVEDM2XsSkBaGV9bDSnRFFhWrQTbmCACqzevE8vth9qhWfQ9SUENXXbLnmMVnBwgW"; // subaddress
                case MoneroNetworkType.TESTNET:
                    return "BhsbVvqW4Wajf4a76QW3hA2B3easR5QdNE5L8NwkY7RWXCrfSuaUwj1DDUsk3XiRGHBqqsK3NPvsATwcmNNPUQQ4SRR2b3V"; // subaddress
                case MoneroNetworkType.MAINNET:
                    return "87a1Yf47UqyQFCrMqqtxfvhJN9se3PgbmU7KUFWqhSu5aih6YsZYoxfjgyxAM1DztNNSdoYTZYn9xa3vHeJjoZqdAybnLzN"; // subaddress
                default:
                    throw new MoneroError("Invalid network type: " + networkType);
            }
        }
    }
}
