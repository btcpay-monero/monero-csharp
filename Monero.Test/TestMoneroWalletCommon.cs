using Monero.Common;
using Monero.Daemon;
using Monero.Test.Utils;
using Monero.Wallet;
using Monero.Wallet.Common;


namespace Monero.Test;

public abstract class TestMoneroWalletCommon
{
    // test constants
    protected static readonly bool LITE_MODE = false;
    protected static readonly bool TEST_NON_RELAYS = true;
    protected static readonly bool TEST_RELAYS = true;
    protected static readonly bool TEST_NOTIFICATIONS = true;
    protected static readonly bool TEST_RESETS = false;
    private static readonly int MAX_TX_PROOFS = 25; // maximum number of transactions to check for each proof, undefined to check all
    private static readonly int SEND_MAX_DIFF = 60;
    private static readonly int SEND_DIVISOR = 10;
    private static readonly int NUM_BLOCKS_LOCKED = 10;

    // instance variables
    protected MoneroWallet wallet = new MoneroWalletRpc("");        // wallet instance to test
    protected MoneroDaemonRpc daemon;     // daemon instance to test

    protected MoneroDaemonRpc GetTestDaemon() { return TestUtils.GetDaemonRpc(); }
    protected abstract MoneroWallet GetTestWallet();
    protected MoneroWallet OpenWallet(string path, string password) { return OpenWallet(new MoneroWalletConfig().SetPath(path).SetPassword(password)); }
    protected abstract MoneroWallet OpenWallet(MoneroWalletConfig config);
    protected abstract MoneroWallet CreateWallet(MoneroWalletConfig config);
    protected void CloseWallet(MoneroWallet wallet) { CloseWallet(wallet, false); }
    protected abstract void CloseWallet(MoneroWallet wallet, bool save);
    protected abstract List<string> GetSeedLanguages();

    #region Begin Tests

    // Can create a random wallet
    [Fact]
    public void TestCreateWalletRandom()
    {
        Assert.True(TEST_NON_RELAYS);
        Exception? e1 = null;  // emulating Java "finally" but compatible with other languages
        try
        {

            // create random wallet
            MoneroWallet wallet = CreateWallet(new MoneroWalletConfig());
            string path = wallet.GetPath();
            Exception e2 = null;
            try
            {
                MoneroUtils.ValidateAddress(wallet.GetPrimaryAddress(), TestUtils.NETWORK_TYPE);
                MoneroUtils.ValidatePrivateViewKey(wallet.GetPrivateViewKey());
                MoneroUtils.ValidatePrivateSpendKey(wallet.GetPrivateSpendKey());
                MoneroUtils.ValidateMnemonic(wallet.GetSeed());
                if (wallet.GetWalletType() != MoneroWalletType.RPC) Assert.True(MoneroWallet.DEFAULT_LANGUAGE == wallet.GetSeedLanguage());  // TODO monero-wallet-rpc: get seed language
            }
            catch (Exception e)
            {
                e2 = e;
            }
            CloseWallet(wallet);
            if (e2 != null) throw e2;

            // attempt to create wallet at same path
            try
            {
                CreateWallet(new MoneroWalletConfig().SetPath(path));
                throw new Exception("Should have thrown error");
            }
            catch (Exception e)
            {
                Assert.True("Wallet already exists: " + path == e.Message);
            }

            // attempt to create wallet with unknown language
            try
            {
                CreateWallet(new MoneroWalletConfig().SetLanguage("english")); // TODO: support lowercase?
                throw new Exception("Should have thrown error");
            }
            catch (Exception e)
            {
                Assert.True("Unknown language: english" == e.Message);
            }
        }
        catch (Exception e)
        {
            e1 = e;
        }

        if (e1 != null) throw new Exception(e1.Message);
    }

    // Can create a wallet from a seed.
    [Fact]
    public void TestCreateWalletFromSeed()
    {
        Assert.True(TEST_NON_RELAYS);
        Exception? e1 = null;  // emulating Java "finally" but compatible with other languages
        try
        {

            // save for comparison
            string primaryAddress = this.wallet.GetPrimaryAddress();
            string privateViewKey = this.wallet.GetPrivateViewKey();
            string privateSpendKey = this.wallet.GetPrivateSpendKey();

            // recreate test wallet from seed
            MoneroWallet wallet = CreateWallet(new MoneroWalletConfig().SetSeed(TestUtils.SEED).SetRestoreHeight(TestUtils.FIRST_RECEIVE_HEIGHT));
            string path = wallet.GetPath();
            Exception e2 = null;
            try
            {
                Assert.True(primaryAddress == wallet.GetPrimaryAddress());
                Assert.True(privateViewKey == wallet.GetPrivateViewKey());
                Assert.True(privateSpendKey == wallet.GetPrivateSpendKey());
                Assert.True(TestUtils.SEED == wallet.GetSeed());
                if (wallet.GetWalletType() != MoneroWalletType.RPC) Assert.True(MoneroWallet.DEFAULT_LANGUAGE == wallet.GetSeedLanguage());
            }
            catch (Exception e)
            {
                e2 = e;
            }
            CloseWallet(wallet);
            if (e2 != null) throw e2;

            // attempt to create wallet with two missing words
            try
            {
                string invalidMnemonic = "memoir desk algebra inbound innocent unplugs fully okay five inflamed giant factual ritual toyed topic snake unhappy guarded tweezers haunted inundate giant";
                wallet = CreateWallet(new MoneroWalletConfig().SetSeed(invalidMnemonic).SetRestoreHeight(TestUtils.FIRST_RECEIVE_HEIGHT));
            }
            catch (Exception e)
            {
                Assert.True("Invalid mnemonic" == e.Message);
            }

            // attempt to create wallet at same path
            try
            {
                CreateWallet(new MoneroWalletConfig().SetPath(path));
                throw new Exception("Should have thrown error");
            }
            catch (Exception e)
            {
                Assert.True("Wallet already exists: " + path == e.Message);
            }
        }
        catch (Exception e)
        {
            e1 = e;
        }

        if (e1 != null) throw new Exception(e1.Message);
    }

    // Can create a wallet from a seed with a seed offset
    [Fact]
    public void TestCreateWalletFromSeedWithOffset()
    {
        Assert.True(TEST_NON_RELAYS);
        Exception e1 = null;  // emulating Java "finally" but compatible with other languages
        try
        {

            // create test wallet with offset
            MoneroWallet wallet = CreateWallet(new MoneroWalletConfig().SetSeed(TestUtils.SEED).SetRestoreHeight(TestUtils.FIRST_RECEIVE_HEIGHT).SetSeedOffset("my secret offset!"));
            Exception e2 = null;
            try
            {
                MoneroUtils.ValidateMnemonic(wallet.GetSeed());
                Assert.True(TestUtils.SEED == wallet.GetSeed());
                MoneroUtils.ValidateAddress(wallet.GetPrimaryAddress(), TestUtils.NETWORK_TYPE);
                Assert.True(TestUtils.ADDRESS != wallet.GetPrimaryAddress());
                if (wallet.GetWalletType() != MoneroWalletType.RPC) Assert.True(MoneroWallet.DEFAULT_LANGUAGE == wallet.GetSeedLanguage());  // TODO monero-wallet-rpc: support
            }
            catch (Exception e)
            {
                e2 = e;
            }
            CloseWallet(wallet);
            if (e2 != null) throw e2;
        }
        catch (Exception e)
        {
            e1 = e;
        }

        if (e1 != null) throw new Exception(e1.Message);
    }

    // Can create a wallet from keys
    [Fact]
    public void TestCreateWalletFromKeys()
    {
        Assert.True(TEST_NON_RELAYS);
        Exception e1 = null; // emulating Java "finally" but compatible with other languages
        try
        {

            // save for comparison
            string primaryAddress = this.wallet.GetPrimaryAddress();
            string privateViewKey = this.wallet.GetPrivateViewKey();
            string privateSpendKey = this.wallet.GetPrivateSpendKey();

            // recreate test wallet from keys
            MoneroWallet wallet = CreateWallet(new MoneroWalletConfig().SetPrimaryAddress(primaryAddress).SetPrivateViewKey(privateViewKey).SetPrivateSpendKey(privateSpendKey).SetRestoreHeight(daemon.GetHeight()));
            string path = wallet.GetPath();
            Exception e2 = null;
            try
            {
                Assert.True(primaryAddress == wallet.GetPrimaryAddress());
                Assert.True(privateViewKey == wallet.GetPrivateViewKey());
                Assert.True(privateSpendKey == wallet.GetPrivateSpendKey());
                if (!wallet.IsConnectedToDaemon()) MoneroUtils.Log(0, "WARNING: wallet created from keys is not connected to authenticated daemon"); // TODO monero-project: keys wallets not connected
                Assert.True(wallet.IsConnectedToDaemon(), "Wallet created from keys is not connected to authenticated daemon");
                if (wallet.GetWalletType() != MoneroWalletType.RPC)
                {
                    MoneroUtils.ValidateMnemonic(wallet.GetSeed()); // TODO monero-wallet-rpc: cannot get seed from wallet created from keys?
                    Assert.True(MoneroWallet.DEFAULT_LANGUAGE == wallet.GetSeedLanguage());
                }
            }
            catch (Exception e)
            {
                e2 = e;
            }
            CloseWallet(wallet);
            if (e2 != null) throw e2;

            // recreate test wallet from spend key
            if (wallet.GetWalletType() != MoneroWalletType.RPC)
            { // TODO monero-wallet-rpc: cannot create wallet from spend key?
                wallet = CreateWallet(new MoneroWalletConfig().SetPrivateSpendKey(privateSpendKey).SetRestoreHeight(daemon.GetHeight()));
                e2 = null;
                try
                {
                    Assert.True(primaryAddress == wallet.GetPrimaryAddress());
                    Assert.True(privateViewKey == wallet.GetPrivateViewKey());
                    Assert.True(privateSpendKey == wallet.GetPrivateSpendKey());
                    if (!wallet.IsConnectedToDaemon()) MoneroUtils.Log(0, "WARNING: wallet created from keys is not connected to authenticated daemon"); // TODO monero-project: keys wallets not connected
                    Assert.True(wallet.IsConnectedToDaemon(), "Wallet created from keys is not connected to authenticated daemon");
                    if (wallet.GetWalletType() != MoneroWalletType.RPC)
                    {
                        MoneroUtils.ValidateMnemonic(wallet.GetSeed()); // TODO monero-wallet-rpc: cannot get seed from wallet created from keys?
                        Assert.True(MoneroWallet.DEFAULT_LANGUAGE == wallet.GetSeedLanguage());
                    }
                }
                catch (Exception e)
                {
                    e2 = e;
                }
                CloseWallet(wallet);
                if (e2 != null) throw e2;
            }

            // attempt to create wallet at same path
            try
            {
                CreateWallet(new MoneroWalletConfig().SetPath(path));
                throw new Exception("Should have thrown error");
            }
            catch (Exception e)
            {
                Assert.True("Wallet already exists: " + path == e.Message);
            }
        }
        catch (Exception e)
        {
            e1 = e;
        }

        if (e1 != null) throw new Exception(e1.Message);
    }

    // Can create wallets with subaddress lookahead
    [Fact]
    public void TestSubaddressLookahead()
    {
        Assert.True(TEST_NON_RELAYS);
        Exception? e1 = null;  // emulating Java "finally" but compatible with other languages
        MoneroWallet? receiver = null;
        try
        {

            // create wallet with high subaddress lookahead
            receiver = CreateWallet(new MoneroWalletConfig().SetAccountLookahead(1).SetSubaddressLookahead(100000));

            // transfer funds to subaddress with high index
            wallet.CreateTx(new MoneroTxConfig()
                .SetAccountIndex(0)
                .AddDestination(receiver.GetSubaddress(0, 85000).GetAddress(), TestUtils.MAX_FEE)
                .SetRelay(true));

            // observe unconfirmed funds
            Thread.Sleep(1000);
            receiver.Sync();
            Assert.True(receiver.GetBalance().CompareTo(0) > 0);
        }
        catch (Exception e)
        {
            e1 = e;
        }

        if (receiver != null) CloseWallet(receiver);
        if (e1 != null) throw new Exception(e1.Message);
    }

    // Can get the wallet's version
    [Fact]
    public void TestGetVersion()
    {
        Assert.True(TEST_NON_RELAYS);
        MoneroVersion version = wallet.GetVersion();
        Assert.True(version.GetNumber() != null);
        Assert.True(version.GetNumber() > 0);
        Assert.True(version.IsRelease() != null);
    }

    // Can get the wallet's path
    [Fact]
    public void TestGetPath()
    {
        Assert.True(TEST_NON_RELAYS);

        // create random wallet
        MoneroWallet wallet = CreateWallet(new MoneroWalletConfig());

        // set a random attribute
        string uuid = Guid.NewGuid().ToString();
        wallet.SetAttribute("uuid", uuid);

        // record the wallet's path then save and close
        string path = wallet.GetPath();
        CloseWallet(wallet, true);

        // re-open the wallet using its path
        wallet = OpenWallet(path, null);

        // test the attribute
        Assert.True(uuid == wallet.GetAttribute("uuid"));
        CloseWallet(wallet);
    }

    // Can set the daemon connection
    [Fact]
    public void TestSetDaemonConnection()
    {
        // create random wallet with default daemon connection
        MoneroWallet wallet = CreateWallet(new MoneroWalletConfig());
        Assert.True(new MoneroRpcConnection(TestUtils.DAEMON_RPC_URI, TestUtils.DAEMON_RPC_USERNAME, TestUtils.DAEMON_RPC_PASSWORD).Equals(wallet.GetDaemonConnection()));
        Assert.True(wallet.IsConnectedToDaemon()); // uses default localhost connection

        // set empty server uri
        wallet.SetDaemonConnection("");
        Assert.Null(wallet.GetDaemonConnection());
        Assert.False(wallet.IsConnectedToDaemon());

        // set offline server uri
        wallet.SetDaemonConnection(TestUtils.OFFLINE_SERVER_URI);
        Assert.True(new MoneroRpcConnection(TestUtils.OFFLINE_SERVER_URI, "", "").Equals(wallet.GetDaemonConnection()));
        Assert.False(wallet.IsConnectedToDaemon());

        // set daemon with wrong credentials
        wallet.SetDaemonConnection(TestUtils.DAEMON_RPC_URI, "wronguser", "wrongpass");
        Assert.True(new MoneroRpcConnection(TestUtils.DAEMON_RPC_URI, "wronguser", "wrongpass").Equals(wallet.GetDaemonConnection()));
        if ("".Equals(TestUtils.DAEMON_RPC_USERNAME) || TestUtils.DAEMON_RPC_USERNAME == null) Assert.True(wallet.IsConnectedToDaemon()); // TODO: monerod without authentication works with bad credentials?
        else Assert.False(wallet.IsConnectedToDaemon());

        // set daemon with authentication
        wallet.SetDaemonConnection(TestUtils.DAEMON_RPC_URI, TestUtils.DAEMON_RPC_USERNAME, TestUtils.DAEMON_RPC_PASSWORD);
        Assert.True(new MoneroRpcConnection(TestUtils.DAEMON_RPC_URI, TestUtils.DAEMON_RPC_USERNAME, TestUtils.DAEMON_RPC_PASSWORD).Equals(wallet.GetDaemonConnection()));
        Assert.True(wallet.IsConnectedToDaemon());

        // nullify daemon connection
        wallet.SetDaemonConnection((string)null);
        Assert.Null(wallet.GetDaemonConnection());
        wallet.SetDaemonConnection(TestUtils.DAEMON_RPC_URI);
        Assert.True(new MoneroRpcConnection(TestUtils.DAEMON_RPC_URI).Equals(wallet.GetDaemonConnection()));
        wallet.SetDaemonConnection((MoneroRpcConnection)null);
        Assert.Null(wallet.GetDaemonConnection());

        // set daemon uri to non-daemon
        wallet.SetDaemonConnection("www.Getmonero.org");
        Assert.True(new MoneroRpcConnection("www.Getmonero.org").Equals(wallet.GetDaemonConnection()));
        Assert.False(wallet.IsConnectedToDaemon());

        // set daemon to invalid uri
        wallet.SetDaemonConnection("abc123");
        Assert.False(wallet.IsConnectedToDaemon());

        // attempt to sync
        try
        {
            wallet.Sync();
            throw new Exception("Exception expected");
        }
        catch (MoneroError e)
        {
            Assert.True("Wallet is not connected to daemon" == e.Message);
        }
        finally
        {
            CloseWallet(wallet);
        }
    }

    // Can use a connection manager
    [Fact]
    public void TestConnectionManager()
    {

        // create connection manager with monerod connections
        MoneroConnectionManager connectionManager = new MoneroConnectionManager();
        MoneroRpcConnection connection1 = new MoneroRpcConnection(TestUtils.GetDaemonRpc().GetRpcConnection()).SetPriority(1);
        MoneroRpcConnection connection2 = new MoneroRpcConnection("localhost:48081").SetPriority(2);
        connectionManager.SetConnection(connection1);
        connectionManager.AddConnection(connection2);

        // create wallet with connection manager
        MoneroWallet wallet = CreateWallet(new MoneroWalletConfig().SetServerUri("").SetConnectionManager(connectionManager));
        Assert.True(TestUtils.GetDaemonRpc().GetRpcConnection() == wallet.GetDaemonConnection());
        Assert.True(wallet.IsConnectedToDaemon());

        // set manager's connection
        connectionManager.SetConnection(connection2);
        Thread.Sleep(TestUtils.AUTO_CONNECT_TIMEOUT_MS);
        Assert.True(connection2 == wallet.GetDaemonConnection());

        // reopen wallet with connection manager
        string path = wallet.GetPath();
        CloseWallet(wallet);
        wallet = OpenWallet(new MoneroWalletConfig().SetServerUri("").SetConnectionManager(connectionManager).SetPath(path));
        Assert.True(connection2 == wallet.GetDaemonConnection());

        // disconnect
        connectionManager.SetConnection((string)null);
        Assert.Null(wallet.GetDaemonConnection());
        Assert.False(wallet.IsConnectedToDaemon());

        // start polling connections
        connectionManager.StartPolling((ulong)TestUtils.SYNC_PERIOD_IN_MS);

        // test that wallet auto connects
        Thread.Sleep(TestUtils.AUTO_CONNECT_TIMEOUT_MS);
        Assert.True(connection1.Equals(wallet.GetDaemonConnection()));
        Assert.True(wallet.IsConnectedToDaemon());

        // test override with bad connection
        wallet.AddListener(new MoneroWalletListener());
        connectionManager.SetAutoSwitch(false);
        connectionManager.SetConnection("http://foo.bar.xyz");
        Assert.True("http://foo.bar.xyz" == wallet.GetDaemonConnection().GetUri());
        Assert.False(wallet.IsConnectedToDaemon());
        Thread.Sleep(5000);
        Assert.False(wallet.IsConnectedToDaemon());

        // set to another connection manager
        MoneroConnectionManager connectionManager2 = new MoneroConnectionManager();
        connectionManager2.SetConnection(connection2);
        wallet.SetConnectionManager(connectionManager2);
        Assert.True(connection2 == wallet.GetDaemonConnection());

        // unset connection manager
        wallet.SetConnectionManager(null);
        Assert.Null(wallet.GetConnectionManager());
        Assert.True(connection2 == wallet.GetDaemonConnection());

        // stop polling and close
        connectionManager.StopPolling();
        CloseWallet(wallet);
    }

    // Can get the seed
    [Fact]
    public void TestGetSeed()
    {
        Assert.True(TEST_NON_RELAYS);
        string seed = wallet.GetSeed();
        MoneroUtils.ValidateMnemonic(seed);
        Assert.True(TestUtils.SEED == seed);
    }

    // Can get the language of the seed
    [Fact]
    public void TestGetSeedLanguage()
    {
        Assert.True(TEST_NON_RELAYS);
        string language = wallet.GetSeedLanguage();
        Assert.True(MoneroWallet.DEFAULT_LANGUAGE == language);
    }

    // Can get a list of supported languages for the seed
    [Fact]
    public void TestGetSeedLanguages()
    {
        Assert.True(TEST_NON_RELAYS);
        List<string> languages = GetSeedLanguages();
        Assert.True(languages.Count > 0);
        foreach (string language in languages) Assert.True(language.Length > 0);
    }

    // Can get the private view key
    [Fact]
    public void TestGetPrivateViewKey()
    {
        Assert.True(TEST_NON_RELAYS);
        string privateViewKey = wallet.GetPrivateViewKey();
        MoneroUtils.ValidatePrivateViewKey(privateViewKey);
    }

    // Can get the private spend key
    [Fact]
    public void TestGetPrivateSpendKey()
    {
        Assert.True(TEST_NON_RELAYS);
        string privateSpendKey = wallet.GetPrivateSpendKey();
        MoneroUtils.ValidatePrivateSpendKey(privateSpendKey);
    }

    // Can get the public view key
    [Fact]
    public void TestGetPublicViewKey()
    {
        Assert.True(TEST_NON_RELAYS);
        string publicViewKey = wallet.GetPublicViewKey();
        MoneroUtils.ValidatePrivateSpendKey(publicViewKey);
    }

    // Can get the public view key
    [Fact]
    public void TestGetPublicSpendKey()
    {
        Assert.True(TEST_NON_RELAYS);
        string publicSpendKey = wallet.GetPublicSpendKey();
        MoneroUtils.ValidatePrivateSpendKey(publicSpendKey);
    }

    // Can get the primary address
    [Fact]
    public void TestGetPrimaryAddress()
    {
        Assert.True(TEST_NON_RELAYS);
        string primaryAddress = wallet.GetPrimaryAddress();
        MoneroUtils.ValidateAddress(primaryAddress, TestUtils.NETWORK_TYPE);
        Assert.True(wallet.GetAddress(0, 0) == primaryAddress);
    }

    // Can get the address of a subaddress at a specified account and subaddress index
    [Fact]
    public void TestGetSubaddressAddress()
    {
        Assert.True(TEST_NON_RELAYS);
        Assert.True(wallet.GetPrimaryAddress() == (wallet.GetSubaddress(0, 0)).GetAddress());
        foreach (MoneroAccount account in wallet.GetAccounts(true))
        {
            foreach (MoneroSubaddress subaddress in account.GetSubaddresses())
            {
                Assert.True(subaddress.GetAddress() == wallet.GetAddress((uint)account.GetIndex(), (uint)subaddress.GetIndex()));
            }
        }
    }

    // Can get addresses out of range of used accounts and subaddresses
    [Fact]
    public void TestGetSubaddressAddressOutOfRange()
    {
        Assert.True(TEST_NON_RELAYS);
        List<MoneroAccount> accounts = wallet.GetAccounts(true);
        uint accountIdx = (uint)accounts.Count - 1;
        uint subaddressIdx = (uint)accounts[accounts.Count - 1].GetSubaddresses().Count;
        string address = wallet.GetAddress(accountIdx, subaddressIdx);
        Assert.NotNull(address);
        Assert.True(address.Length > 0);
    }

    #endregion
}