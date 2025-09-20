using Monero.Common;
using Monero.Daemon;
using Monero.IntegrationTests.Utils;
using Monero.Wallet;
using Monero.Wallet.Common;

namespace Monero.IntegrationTests;

public abstract class TestMoneroWalletCommon
{
    // test constants
    protected static readonly bool LITE_MODE = false;
    protected static readonly bool TEST_NON_RELAYS = true;
    protected static readonly bool TEST_RELAYS = true;
    protected static readonly bool TEST_NOTIFICATIONS = true;
    protected static readonly bool TEST_RESETS = false;

    protected MoneroDaemonRpc daemon; // daemon instance to test

    // instance variables
    protected IMoneroWallet wallet = new MoneroWalletRpc(""); // wallet instance to test

    protected TestMoneroWalletCommon()
    {
        daemon = TestUtils.GetDaemonRpc();
    }

    protected MoneroDaemonRpc GetTestDaemon() { return TestUtils.GetDaemonRpc(); }
    protected abstract Task<IMoneroWallet> GetTestWallet();

    protected async Task<IMoneroWallet> OpenWallet(string path, string? password)
    {
        return await OpenWallet(new MoneroWalletConfig().SetPath(path).SetPassword(password));
    }

    protected abstract Task<IMoneroWallet> OpenWallet(MoneroWalletConfig config);
    protected abstract Task<IMoneroWallet> CreateWallet(MoneroWalletConfig config);
    private async Task CloseWallet(IMoneroWallet walletInstance) { await CloseWallet(walletInstance, false); }
    protected abstract Task CloseWallet(IMoneroWallet walletInstance, bool save);
    protected abstract Task<List<string>> GetSeedLanguages();

    private async Task TestWallet(Func<Task> action, IMoneroWallet moneroWallet)
    {
        await TestWallet(action, moneroWallet, false);
    }

    private async Task TestWallet(Func<Task> action, IMoneroWallet moneroWallet, bool checkSeed)
    {
        Exception? e2 = null;
        try
        {
            await action();

            if (moneroWallet.GetWalletType() != MoneroWalletType.Rpc)
            {
                Assert.True(IMoneroWallet.DefaultLanguage == await moneroWallet.GetSeedLanguage());

                if (checkSeed)
                {
                    MoneroUtils.ValidateMnemonic(await moneroWallet.GetSeed());
                }
            }
        }
        catch (Exception e)
        {
            e2 = e;
        }

        await CloseWallet(moneroWallet);

        if (e2 != null)
        {
            throw e2;
        }
    }

    // Can create a random wallet
    [Fact]
    public async Task TestCreateWalletRandom()
    {
        Assert.True(TEST_NON_RELAYS);
        Exception? e1 = null; // emulating Java "finally" but compatible with other languages
        try
        {
            // create random wallet
            IMoneroWallet moneroWallet = await CreateWallet(new MoneroWalletConfig());
            string path = await moneroWallet.GetPath();

            await TestWallet(async () =>
            {
                MoneroUtils.ValidateAddress(await moneroWallet.GetPrimaryAddress());
                MoneroUtils.ValidatePrivateViewKey(await moneroWallet.GetPrivateViewKey());
                MoneroUtils.ValidatePrivateSpendKey(await moneroWallet.GetPrivateSpendKey());
                MoneroUtils.ValidateMnemonic(await moneroWallet.GetSeed());
            }, moneroWallet);

            // attempt to create wallet at same path
            try
            {
                await CreateWallet(new MoneroWalletConfig().SetPath(path));
                throw new Exception("Should have thrown error");
            }
            catch (Exception e)
            {
                Assert.True("Wallet already exists: " + path == e.Message);
            }

            // attempt to create wallet with unknown language
            try
            {
                await CreateWallet(new MoneroWalletConfig().SetLanguage("english")); // TODO: support lowercase?
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

        if (e1 != null)
        {
            throw new Exception(e1.Message);
        }
    }

    // Can create a wallet from a seed.
    [Fact]
    public async Task TestCreateWalletFromSeed()
    {
        Assert.True(TEST_NON_RELAYS);
        Exception? e1 = null; // emulating Java "finally" but compatible with other languages
        try
        {
            // save for comparison
            string primaryAddress = await wallet.GetPrimaryAddress();
            string privateViewKey = await wallet.GetPrivateViewKey();
            string privateSpendKey = await wallet.GetPrivateSpendKey();

            // recreate the test wallet from seed
            IMoneroWallet moneroWallet = await CreateWallet(new MoneroWalletConfig().SetSeed(TestUtils.SEED)
                .SetRestoreHeight(TestUtils.FIRST_RECEIVE_HEIGHT));
            string path = await moneroWallet.GetPath();

            await TestWallet(async () =>
            {
                Assert.True(primaryAddress == await moneroWallet.GetPrimaryAddress());
                Assert.True(privateViewKey == await moneroWallet.GetPrivateViewKey());
                Assert.True(privateSpendKey == await moneroWallet.GetPrivateSpendKey());
                Assert.True(TestUtils.SEED == await moneroWallet.GetSeed());
            }, moneroWallet);

            // attempt to create a wallet with two missing words
            try
            {
                string invalidMnemonic =
                    "memoir desk algebra inbound innocent unplugs fully okay five inflamed giant factual ritual toyed topic snake unhappy guarded tweezers haunted inundate giant";
                await CreateWallet(new MoneroWalletConfig().SetSeed(invalidMnemonic)
                    .SetRestoreHeight(TestUtils.FIRST_RECEIVE_HEIGHT));
            }
            catch (Exception e)
            {
                Assert.True("Invalid mnemonic" == e.Message);
            }

            // attempt to create a wallet at the same path
            try
            {
                await CreateWallet(new MoneroWalletConfig().SetPath(path));
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

        if (e1 != null)
        {
            throw new Exception(e1.Message);
        }
    }

    // Can create a wallet from a seed with a seed offset
    [Fact]
    public async Task TestCreateWalletFromSeedWithOffset()
    {
        Assert.True(TEST_NON_RELAYS);
        Exception? e1 = null; // emulating Java "finally" but compatible with other languages
        try
        {
            // create a test wallet with offset
            IMoneroWallet moneroWallet = await CreateWallet(new MoneroWalletConfig().SetSeed(TestUtils.SEED)
                .SetRestoreHeight(TestUtils.FIRST_RECEIVE_HEIGHT).SetSeedOffset("my secret offset!"));

            await TestWallet(async () =>
            {
                MoneroUtils.ValidateMnemonic(await moneroWallet.GetSeed());
                Assert.True(TestUtils.SEED == await moneroWallet.GetSeed());
                MoneroUtils.ValidateAddress(await moneroWallet.GetPrimaryAddress());
                Assert.True(TestUtils.ADDRESS != await moneroWallet.GetPrimaryAddress());
            }, moneroWallet);
        }
        catch (Exception e)
        {
            e1 = e;
        }

        if (e1 != null)
        {
            throw new Exception(e1.Message);
        }
    }

    // Can create a wallet from keys
    [Fact]
    public async Task TestCreateWalletFromKeys()
    {
        Assert.True(TEST_NON_RELAYS);
        Exception? e1 = null; // emulating Java "finally" but compatible with other languages
        try
        {
            // save for comparison
            string primaryAddress = await wallet.GetPrimaryAddress();
            string privateViewKey = await wallet.GetPrivateViewKey();
            string privateSpendKey = await wallet.GetPrivateSpendKey();

            // recreate the test wallet from keys
            IMoneroWallet moneroWallet = await CreateWallet(new MoneroWalletConfig().SetPrimaryAddress(primaryAddress)
                .SetPrivateViewKey(privateViewKey).SetPrivateSpendKey(privateSpendKey)
                .SetRestoreHeight(await daemon.GetHeight()));
            string path = await moneroWallet.GetPath();

            Func<Task> action = async () =>
            {
                Assert.True(primaryAddress == await moneroWallet.GetPrimaryAddress());
                Assert.True(privateViewKey == await moneroWallet.GetPrivateViewKey());
                Assert.True(privateSpendKey == await moneroWallet.GetPrivateSpendKey());
                Assert.True(await moneroWallet.IsConnectedToDaemon(),
                    "Wallet created from keys is not connected to authenticated daemon");
            };

            await TestWallet(action, moneroWallet, true);

            // recreate test wallet from spend key
            if (moneroWallet.GetWalletType() != MoneroWalletType.Rpc)
            {
                // TODO monero-wallet-rpc: cannot create wallet from spend key?
                moneroWallet = await CreateWallet(new MoneroWalletConfig().SetPrivateSpendKey(privateSpendKey)
                    .SetRestoreHeight(await daemon.GetHeight()));
                await TestWallet(action, moneroWallet, true);
            }

            // attempt to create wallet at same path
            try
            {
                await CreateWallet(new MoneroWalletConfig().SetPath(path));
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

        if (e1 != null)
        {
            throw new Exception(e1.Message);
        }
    }

    // Can create wallets with subaddress lookahead
    [Fact]
    public async Task TestSubaddressLookahead()
    {
        Assert.True(TEST_NON_RELAYS);
        Exception? e1 = null; // emulating Java "finally" but compatible with other languages
        IMoneroWallet? receiver = null;
        try
        {
            // create wallet with high subaddress lookahead
            receiver = await CreateWallet(
                new MoneroWalletConfig().SetAccountLookahead(1).SetSubaddressLookahead(100000));

            // transfer funds to subaddress with high index
            await wallet.CreateTx(new MoneroTxConfig()
                .SetAccountIndex(0)
                .AddDestination((await receiver.GetSubaddress(0, 85000)).GetAddress()!, TestUtils.MAX_FEE)
                .SetRelay(true));

            // observe unconfirmed funds
            Thread.Sleep(1000);
            await receiver.Sync();
            Assert.True((await receiver.GetBalance()).CompareTo(0) > 0);
        }
        catch (Exception e)
        {
            e1 = e;
        }

        if (receiver != null)
        {
            await CloseWallet(receiver);
        }

        if (e1 != null)
        {
            throw new Exception(e1.Message);
        }
    }

    // Can get the wallet's version
    [Fact]
    public async Task TestGetVersion()
    {
        Assert.True(TEST_NON_RELAYS);
        MoneroVersion version = await wallet.GetVersion();
        Assert.True(version.GetNumber() != null);
        Assert.True(version.GetNumber() > 0);
        Assert.True(version.IsRelease() != null);
    }

    // Can get the wallet's path
    [Fact]
    public async Task TestGetPath()
    {
        Assert.True(TEST_NON_RELAYS);

        // create a random wallet
        IMoneroWallet moneroWallet = await CreateWallet(new MoneroWalletConfig());

        // set a random attribute
        string uuid = Guid.NewGuid().ToString();
        await moneroWallet.SetAttribute("uuid", uuid);

        // record the wallet's path, then save and close
        string path = await moneroWallet.GetPath();
        await CloseWallet(moneroWallet, true);

        // re-open the wallet using its path
        moneroWallet = await OpenWallet(path, null);

        // test the attribute
        Assert.True(uuid == await moneroWallet.GetAttribute("uuid"));
        await CloseWallet(moneroWallet);
    }

    // Can set the daemon connection
    [Fact]
    public async Task TestSetDaemonConnection()
    {
        // create random wallet with default daemon connection
        IMoneroWallet moneroWallet = await CreateWallet(new MoneroWalletConfig());
        Assert.True(
            new MoneroRpcConnection(TestUtils.DAEMON_RPC_URI, TestUtils.DAEMON_RPC_USERNAME,
                TestUtils.DAEMON_RPC_PASSWORD).Equals(await moneroWallet.GetDaemonConnection()));
        Assert.True(await moneroWallet.IsConnectedToDaemon()); // uses default localhost connection

        // set empty server uri
        await moneroWallet.SetDaemonConnection("");
        Assert.Null(await moneroWallet.GetDaemonConnection());
        Assert.False(await moneroWallet.IsConnectedToDaemon());

        // set offline server uri
        await moneroWallet.SetDaemonConnection(TestUtils.OFFLINE_SERVER_URI);
        Assert.True(
            new MoneroRpcConnection(TestUtils.OFFLINE_SERVER_URI, "", "").Equals(
                await moneroWallet.GetDaemonConnection()));
        Assert.False(await moneroWallet.IsConnectedToDaemon());

        // set daemon with wrong credentials
        await moneroWallet.SetDaemonConnection(TestUtils.DAEMON_RPC_URI, "wronguser", "wrongpass");
        Assert.True(
            new MoneroRpcConnection(TestUtils.DAEMON_RPC_URI, "wronguser", "wrongpass").Equals(
                await moneroWallet.GetDaemonConnection()));
        if (string.IsNullOrEmpty(TestUtils.DAEMON_RPC_USERNAME))
        {
            Assert.True(await moneroWallet
                .IsConnectedToDaemon()); // TODO: monerod without authentication works with bad credentials?
        }
        else
        {
            Assert.False(await moneroWallet.IsConnectedToDaemon());
        }

        // set daemon with authentication
        await moneroWallet.SetDaemonConnection(TestUtils.DAEMON_RPC_URI, TestUtils.DAEMON_RPC_USERNAME,
            TestUtils.DAEMON_RPC_PASSWORD);
        Assert.True(
            new MoneroRpcConnection(TestUtils.DAEMON_RPC_URI, TestUtils.DAEMON_RPC_USERNAME,
                TestUtils.DAEMON_RPC_PASSWORD).Equals(await moneroWallet.GetDaemonConnection()));
        Assert.True(await moneroWallet.IsConnectedToDaemon());

        // nullify daemon connection
        await moneroWallet.SetDaemonConnection((MoneroRpcConnection?)null);
        Assert.Null(await moneroWallet.GetDaemonConnection());
        await moneroWallet.SetDaemonConnection(TestUtils.DAEMON_RPC_URI);
        Assert.True(new MoneroRpcConnection(TestUtils.DAEMON_RPC_URI).Equals(await moneroWallet.GetDaemonConnection()));
        await moneroWallet.SetDaemonConnection((MoneroRpcConnection?)null);
        Assert.Null(await moneroWallet.GetDaemonConnection());

        // set daemon uri to non-daemon
        await moneroWallet.SetDaemonConnection("www.Getmonero.org");
        Assert.True(new MoneroRpcConnection("www.Getmonero.org").Equals(await moneroWallet.GetDaemonConnection()));
        Assert.False(await moneroWallet.IsConnectedToDaemon());

        // set daemon to invalid uri
        await moneroWallet.SetDaemonConnection("abc123");
        Assert.False(await moneroWallet.IsConnectedToDaemon());

        // attempt to sync
        try
        {
            await moneroWallet.Sync();
            throw new Exception("Exception expected");
        }
        catch (MoneroError e)
        {
            Assert.Equal("Wallet is not connected to daemon", e.Message);
        }
        finally
        {
            await CloseWallet(moneroWallet);
        }
    }

    // Can get the seed
    [Fact]
    public async Task TestGetSeed()
    {
        Assert.True(TEST_NON_RELAYS);
        string seed = await wallet.GetSeed();
        MoneroUtils.ValidateMnemonic(seed);
        Assert.True(TestUtils.SEED == seed);
    }

    // Can get the language of the seed
    [Fact]
    public async Task TestGetSeedLanguage()
    {
        Assert.True(TEST_NON_RELAYS);
        string language = await wallet.GetSeedLanguage();
        Assert.True(IMoneroWallet.DefaultLanguage == language);
    }

    // Can get a list of supported languages for the seed
    [Fact]
    public async Task TestGetSeedLanguages()
    {
        Assert.True(TEST_NON_RELAYS);
        List<string> languages = await GetSeedLanguages();
        Assert.True(languages.Count > 0);
        foreach (string language in languages)
        {
            Assert.True(language.Length > 0);
        }
    }

    // Can get the private view key
    [Fact]
    public async Task TestGetPrivateViewKey()
    {
        Assert.True(TEST_NON_RELAYS);
        string privateViewKey = await wallet.GetPrivateViewKey();
        MoneroUtils.ValidatePrivateViewKey(privateViewKey);
    }

    // Can get the private spend key
    [Fact]
    public async Task TestGetPrivateSpendKey()
    {
        Assert.True(TEST_NON_RELAYS);
        string privateSpendKey = await wallet.GetPrivateSpendKey();
        MoneroUtils.ValidatePrivateSpendKey(privateSpendKey);
    }

    // Can get the public view key
    [Fact]
    public async Task TestGetPublicViewKey()
    {
        Assert.True(TEST_NON_RELAYS);
        string publicViewKey = await wallet.GetPublicViewKey();
        MoneroUtils.ValidatePrivateSpendKey(publicViewKey);
    }

    // Can get the public view key
    [Fact]
    public async Task TestGetPublicSpendKey()
    {
        Assert.True(TEST_NON_RELAYS);
        string publicSpendKey = await wallet.GetPublicSpendKey();
        MoneroUtils.ValidatePrivateSpendKey(publicSpendKey);
    }

    // Can get the primary address
    [Fact]
    public async Task TestGetPrimaryAddress()
    {
        Assert.True(TEST_NON_RELAYS);
        string primaryAddress = await wallet.GetPrimaryAddress();
        MoneroUtils.ValidateAddress(primaryAddress);
        Assert.True(await wallet.GetAddress(0, 0) == primaryAddress);
    }

    // Can get the address of a subaddress at a specified account and subaddress index
    [Fact]
    public async Task TestGetSubaddressAddress()
    {
        Assert.True(TEST_NON_RELAYS);
        Assert.True(await wallet.GetPrimaryAddress() == (await wallet.GetSubaddress(0, 0)).GetAddress());
        foreach (MoneroAccount account in await wallet.GetAccounts(true))
        {
            foreach (MoneroSubaddress subaddress in account.GetSubaddresses()!)
            {
                Assert.True(subaddress.GetAddress() ==
                            await wallet.GetAddress((uint)account.GetIndex()!, (uint)subaddress.GetIndex()!));
            }
        }
    }

    // Can get addresses out of range of used accounts and subaddresses
    [Fact]
    public async Task TestGetSubaddressAddressOutOfRange()
    {
        Assert.True(TEST_NON_RELAYS);
        List<MoneroAccount> accounts = await wallet.GetAccounts(true);
        uint accountIdx = (uint)accounts.Count - 1;
        uint subaddressIdx = (uint)accounts[accounts.Count - 1].GetSubaddresses()!.Count;
        string address = await wallet.GetAddress(accountIdx, subaddressIdx);
        Assert.NotNull(address);
        Assert.True(address.Length > 0);
    }
}