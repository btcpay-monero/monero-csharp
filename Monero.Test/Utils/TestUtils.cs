using Monero.Common;
using Monero.Daemon;
using Monero.Wallet;

namespace Monero.Test.Utils
{
    internal abstract class TestUtils
    {
        // directory with monero binaries to test (monerod and monero-wallet-rpc)
        public static readonly string MONERO_BINS_DIR = "";
  
        // monero daemon rpc endpoint configuration (change per your configuration)
        public static readonly string DAEMON_RPC_URI = "http://monerod:18081";
        public static readonly string DAEMON_RPC_USERNAME = "";
        public static readonly string DAEMON_RPC_PASSWORD = "";
        public static readonly string DAEMON_LOCAL_PATH = MONERO_BINS_DIR + "/monerod";
  
        // monero wallet rpc configuration (change per your configuration)
        public static readonly int WALLET_RPC_PORT_START = 18082; // test wallet executables will bind to consecutive ports after these
        public static readonly bool WALLET_RPC_ZMQ_ENABLED = false;
        public static readonly int WALLET_RPC_ZMQ_PORT_START = 58083;
        public static readonly int WALLET_RPC_ZMQ_BIND_PORT_START = 48083;  // TODO: zmq bind port necessary?
        public static readonly string WALLET_RPC_USERNAME = "";
        public static readonly string WALLET_RPC_PASSWORD = "";
        public static readonly string WALLET_RPC_ZMQ_DOMAIN = "127.0.0.1";
        public static readonly string WALLET_RPC_DOMAIN = "http://xmr_wallet";
        public static readonly string WALLET_RPC_URI = WALLET_RPC_DOMAIN + ":" + WALLET_RPC_PORT_START;
        public static readonly string WALLET_RPC_ZMQ_URI = "tcp://" + WALLET_RPC_ZMQ_DOMAIN + ":" + WALLET_RPC_ZMQ_PORT_START;
        public static readonly string WALLET_RPC_LOCAL_PATH = MONERO_BINS_DIR + "/monero-wallet-rpc";
        public static readonly string WALLET_RPC_LOCAL_WALLET_DIR = MONERO_BINS_DIR;
        public static readonly string WALLET_RPC_ACCESS_CONTROL_ORIGINS = "http://localhost:8080"; // cors access from web browser
  
        // test wallet config
        public static readonly string WALLET_NAME = "test_wallet_1";
        public static readonly string WALLET_PASSWORD = "supersecretpassword123";
        public static readonly string TEST_WALLETS_DIR = "./test_wallets";
        public static readonly string WALLET_FULL_PATH = TEST_WALLETS_DIR + "/" + WALLET_NAME;
  
        // test wallet constants
        public static readonly ulong MAX_FEE = 75000000000;
        public static readonly MoneroNetworkType NETWORK_TYPE = MoneroNetworkType.MAINNET;
        public static readonly string LANGUAGE = "English";
        public static readonly string SEED = "silk mocked cucumber lettuce hope adrenalin aching lush roles fuel revamp baptism wrist ulong tender teardrop midst pastry pigment equip frying inbound pinched ravine frying";
        public static readonly string ADDRESS = "A1y9sbVt8nqhZAVm3me1U18rUVXcjeNKuBd1oE2cTs8biA9cozPMeyYLhe77nPv12JA3ejJN3qprmREriit2fi6tJDi99RR";
        public static readonly ulong FIRST_RECEIVE_HEIGHT = 171; // NOTE: this value must be the height of the wallet's first tx for tests
        public static readonly int SYNC_PERIOD_IN_MS = 5000; // period between wallet syncs in milliseconds
        public static readonly string OFFLINE_SERVER_URI = "offline_server_uri"; // dummy server uri to remain offline because wallet2 connects to default if not given
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
            throw new NotImplementedException();
        }

        public static MoneroWalletRpc GetWalletRpc()
        {
            throw new NotImplementedException();
        }

    }
}
