using Monero.Common;
using Monero.Daemon;
using Monero.IntegrationTests.Utils;
using Monero.Wallet;
using Monero.Wallet.Common;

using Xunit;

namespace Monero.IntegrationTests;

public class MoneroWalletRpcIntegrationTest
{
    private readonly MoneroDaemonRpc _daemon = TestUtils.GetDaemonRpc(); // daemon instance to test

    private readonly MoneroWalletRpc _wallet = TestUtils.GetWalletRpc().Result; // wallet instance to test

    private async Task<IMoneroWallet> OpenWallet(MoneroWalletConfig? config)
    {
        MoneroWalletRpc ret;
        // assign defaults
        if (config == null)
        {
            config = new MoneroWalletConfig();
        }

        if (config.GetPassword() == null)
        {
            config.SetPassword(TestUtils.WALLET_PASSWORD);
        }

        if (config.GetServer() == null)
        {
            config.SetServer(_daemon.GetRpcConnection());
        }

        // create a client connected to an internal monero-wallet-rpc process
        MoneroWalletRpc moneroWalletRpc = await TestUtils.GetCreateWallet();

        // open wallet
        try
        {
            await moneroWalletRpc.OpenWallet(config);
            await moneroWalletRpc.SetDaemonConnection(await moneroWalletRpc.GetDaemonConnection(), true,
                null); // set daemon as trusted
            if (await moneroWalletRpc.IsConnectedToDaemon())
            {
                await moneroWalletRpc.StartSyncing((ulong)TestUtils.SYNC_PERIOD_IN_MS);
            }

            ret = moneroWalletRpc;
        }
        catch (MoneroError)
        {
            try { TestUtils.StopWalletRpcProcess(moneroWalletRpc); }
            catch (Exception e2) { throw new Exception(e2.Message); }

            throw;
        }

        return ret;
    }

    private async Task<IMoneroWallet> CreateWallet(MoneroWalletConfig? config)
    {
        // assign defaults
        if (config == null)
        {
            config = new MoneroWalletConfig();
        }

        bool random = config.GetSeed() == null && config.GetPrimaryAddress() == null;

        if (config.GetPath() == null)
        {
            config.SetPath(GenUtils.GetGuid());
        }

        if (config.GetPassword() == null)
        {
            config.SetPassword(TestUtils.WALLET_PASSWORD);
        }

        if (config.GetRestoreHeight() == null && !random)
        {
            config.SetRestoreHeight(0);
        }

        if (config.GetServer() == null)
        {
            config.SetServer(_daemon.GetRpcConnection());
        }

        // create a client connected to xmr_wallet_2 container
        MoneroWalletRpc walletRpc = await TestUtils.GetCreateWallet();

        // create wallet
        try
        {
            await walletRpc.CreateWallet(config);
            await walletRpc.SetDaemonConnection(await walletRpc.GetDaemonConnection(), true,
                null); // set daemon as trusted
            if (await walletRpc.IsConnectedToDaemon())
            {
                await walletRpc.StartSyncing((ulong)TestUtils.SYNC_PERIOD_IN_MS);
            }

            return walletRpc;
        }
        catch (MoneroError e)
        {
            if (!e.Message.ToLower().Contains("already exists"))
            {
                try { await CloseWallet(walletRpc, false); }
                catch (Exception e2)
                {
                    throw new Exception("An error occurred while stopping monero wallet rpc process", e2);
                }
            }

            throw;
        }
    }

    private async Task CloseWallet(IMoneroWallet walletInstance) { await CloseWallet(walletInstance, false); }

    private async Task CloseWallet(IMoneroWallet walletInstance, bool save)
    {
        MoneroWalletRpc walletRpc = (MoneroWalletRpc)walletInstance;
        string walletPath = await walletRpc.GetPath();

        if (string.IsNullOrEmpty(walletPath))
        {
            return;
        }

        await walletRpc.Close(save);
    }

    private Task<List<string>> GetSeedLanguages()
    {
        throw new NotImplementedException();
    }

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

            if (checkSeed)
            {
                MoneroUtils.ValidateMnemonic(await moneroWallet.GetSeed());
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
        Exception? e1 = null; // emulating Java "finally" but compatible with other languages
        try
        {
            // create a random wallet
            IMoneroWallet moneroWallet = await CreateWallet(new MoneroWalletConfig());
            string path = await moneroWallet.GetPath();

            await TestWallet(async () =>
            {
                MoneroUtils.ValidateAddress(await moneroWallet.GetPrimaryAddress());
                MoneroUtils.ValidatePrivateViewKey(await moneroWallet.GetPrivateViewKey());
                MoneroUtils.ValidatePrivateSpendKey(await moneroWallet.GetPrivateSpendKey());
                MoneroUtils.ValidateMnemonic(await moneroWallet.GetSeed());
            }, moneroWallet);

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

            // attempt to create wallet with unknown language
            try
            {
                await CreateWallet(new MoneroWalletConfig().SetLanguage("english")); // TODO: support lowercase?
                throw new Exception("Should have thrown error");
            }
            catch (Exception e)
            {
                Assert.Equal("Unknown language: english", e.Message);
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
        Exception? e1 = null; // emulating Java "finally" but compatible with other languages
        try
        {
            // save for comparison
            string primaryAddress = await _wallet.GetPrimaryAddress();
            string privateViewKey = await _wallet.GetPrivateViewKey();
            string privateSpendKey = await _wallet.GetPrivateSpendKey();

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
                Assert.Equal("Invalid mnemonic", e.Message);
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
        Exception? e1 = null; // emulating Java "finally" but compatible with other languages
        try
        {
            // create a test wallet with offset
            IMoneroWallet moneroWallet = await CreateWallet(new MoneroWalletConfig().SetSeed(TestUtils.SEED)
                .SetRestoreHeight(TestUtils.FIRST_RECEIVE_HEIGHT).SetSeedOffset("my secret offset!"));

            await TestWallet(async () =>
            {
                MoneroUtils.ValidateMnemonic(await moneroWallet.GetSeed());
                Assert.True(TestUtils.SEED != await moneroWallet.GetSeed());
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
        Exception? e1 = null; // emulating Java "finally" but compatible with other languages
        try
        {
            // save for comparison
            string primaryAddress = await _wallet.GetPrimaryAddress();
            string privateViewKey = await _wallet.GetPrivateViewKey();
            string privateSpendKey = await _wallet.GetPrivateSpendKey();

            // recreate the test wallet from keys
            IMoneroWallet moneroWallet = await CreateWallet(new MoneroWalletConfig().SetPrimaryAddress(primaryAddress)
                .SetPrivateViewKey(privateViewKey).SetPrivateSpendKey(privateSpendKey)
                .SetRestoreHeight(await _daemon.GetHeight()));
            string path = await moneroWallet.GetPath();

            Func<Task> action = async () =>
            {
                Assert.True(primaryAddress == await moneroWallet.GetPrimaryAddress());
                Assert.True(privateViewKey == await moneroWallet.GetPrivateViewKey());
                Assert.True(privateSpendKey == await moneroWallet.GetPrivateSpendKey());
                if (TestUtils.TESTS_INCONTAINER)
                {
                    Assert.True(await moneroWallet.IsConnectedToDaemon(),
                        "Wallet created from keys is not connected to authenticated daemon");
                }
            };

            await TestWallet(action, moneroWallet, false);

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

    // Can get the wallet's version
    [Fact]
    public async Task TestGetVersion()
    {
        MoneroVersion version = await _wallet.GetVersion();
        Assert.NotNull(version.GetNumber());
        Assert.True(version.GetNumber() > 0);
        Assert.NotNull(version.IsRelease());
    }

    // Can get the wallet's path
    [Fact]
    public async Task TestGetPath()
    {
        // create a random wallet
        IMoneroWallet moneroWallet = await CreateWallet(new MoneroWalletConfig());

        // set a random attribute
        string uuid = Guid.NewGuid().ToString();
        await moneroWallet.SetAttribute("uuid", uuid);

        // record the wallet's path, then save and close
        string path = await moneroWallet.GetPath();
        await CloseWallet(moneroWallet, true);

        // re-open the wallet using its path
        moneroWallet = await OpenWallet(new MoneroWalletConfig().SetPath(path));

        // test the attribute
        Assert.True(uuid == await moneroWallet.GetAttribute("uuid"));
        await CloseWallet(moneroWallet);
    }

    // Can get the seed
    [Fact]
    public async Task TestGetSeed()
    {
        string seed = await _wallet.GetSeed();
        MoneroUtils.ValidateMnemonic(seed);
        Assert.True(TestUtils.SEED == seed);
    }

    // Can get the language of the seed
    [Fact(Skip = "monero-wallet-rpc does not support getting seed language")]
    public async Task TestGetSeedLanguage()
    {
        string language = await _wallet.GetSeedLanguage();
        Assert.True(IMoneroWallet.DefaultLanguage == language);
    }

    // Can get a list of supported languages for the seed
    [Fact(Skip = "monero-wallet-rpc does not support getting seed languages")]
    public async Task TestGetSeedLanguages()
    {
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
        string privateViewKey = await _wallet.GetPrivateViewKey();
        MoneroUtils.ValidatePrivateViewKey(privateViewKey);
    }

    // Can get the private spent key
    [Fact]
    public async Task TestGetPrivateSpendKey()
    {
        string privateSpendKey = await _wallet.GetPrivateSpendKey();
        MoneroUtils.ValidatePrivateSpendKey(privateSpendKey);
    }

    // Can get the public view key
    [Fact(Skip = "Enable after monero-project fix (https://github.com/monero-project/monero/pull/9364)")]
    public async Task TestGetPublicViewKey()
    {
        string publicViewKey = await _wallet.GetPublicViewKey();
        MoneroUtils.ValidatePrivateSpendKey(publicViewKey);
    }

    // Can get the public view key
    [Fact(Skip = "Enable after monero-project fix (https://github.com/monero-project/monero/pull/9364)")]
    public async Task TestGetPublicSpendKey()
    {
        string publicSpendKey = await _wallet.GetPublicSpendKey();
        MoneroUtils.ValidatePrivateSpendKey(publicSpendKey);
    }

    // Can get the primary address
    [Fact]
    public async Task TestGetPrimaryAddress()
    {
        string primaryAddress = await _wallet.GetPrimaryAddress();
        MoneroUtils.ValidateAddress(primaryAddress);
        Assert.True(await _wallet.GetAddress(0, 0) == primaryAddress);
    }

    // Can get the address of a subaddress at a specified account and subaddress index
    [Fact]
    public async Task TestGetSubaddressAddress()
    {
        Assert.True(await _wallet.GetPrimaryAddress() == (await _wallet.GetSubaddress(0, 0)).GetAddress());
        foreach (MoneroAccount account in await _wallet.GetAccounts(true, true, null))
        {
            foreach (MoneroSubaddress subaddress in account.GetSubaddresses()!)
            {
                Assert.True(subaddress.GetAddress() ==
                            await _wallet.GetAddress((uint)account.GetIndex()!, (uint)subaddress.GetIndex()!));
            }
        }
    }

    // Can get addresses out of range of used accounts and subaddresses
    [Fact]
    public async Task TestGetSubaddressAddressOutOfRange()
    {
        List<MoneroAccount> accounts = await _wallet.GetAccounts(true, true, null);
        int accountIdx = accounts.Count - 1;
        MoneroAccount account = accounts[accountIdx];
        List<MoneroSubaddress>? subaddresses = account.GetSubaddresses();

        if (subaddresses == null)
        {
            throw new MoneroError("Subaddresses is null");
        }

        int subaddressIdx = subaddresses.Count;
        string? address = await _wallet.GetAddress((uint)accountIdx, (uint)subaddressIdx);
        Assert.Null(address);
    }


    // Can get the current height that the wallet is synchronized to
    [Fact]
    public async Task TestGetHeight()
    {
        ulong lastHeight = await _wallet.GetHeight();

        await _daemon.WaitForNextBlockHeader();

        ulong currentHeight = await _wallet.GetHeight();

        Assert.True(currentHeight > lastHeight);
    }

    // Can create a new account without a label
    [Fact]
    public async Task TestCreateAccountWithoutLabel()
    {
        List<MoneroAccount> accountsBefore = await _wallet.GetAccounts(false, true, null);
        MoneroAccount createdAccount = await _wallet.CreateAccount(null);
        TestAccount(createdAccount);
        Assert.Equal(accountsBefore.Count, (await _wallet.GetAccounts(false, true, null)).Count - 1);
    }

    // Can get accounts without subaddresses
    [Fact]
    public async Task TestGetAccountsWithoutSubaddresses()
    {
        List<MoneroAccount> accounts = await _wallet.GetAccounts(false, true, null);
        Assert.NotEmpty(accounts);
        foreach (MoneroAccount account in accounts)
        {
            TestAccount(account);
            Assert.Null(account.GetSubaddresses());
        }
    }

    // Can get accounts with subaddresses
    [Fact]
    public async Task TestGetAccountsWithSubaddresses()
    {
        List<MoneroAccount> accounts = await _wallet.GetAccounts(true, false, null);
        Assert.NotEmpty(accounts);
        foreach (MoneroAccount account in accounts)
        {
            TestAccount(account);
            List<MoneroSubaddress> subaddresses = account.GetSubaddresses() ?? [];
            Assert.NotEmpty(subaddresses);
        }
    }

    // Can set account labels
    [Fact]
    public async Task TestSetAccountLabel()
    {
        // create an account
        if ((await _wallet.GetAccounts(false, true, null)).Count < 2)
        {
            // create an account
            if ((await _wallet.GetAccounts(false, true, null)).Count < 2)
            {
                await _wallet.CreateAccount(null);
            }
        }

        // set account label
        string label = GenUtils.GetGuid();
        await _wallet.SetAccountLabel(1, 0, label);
        Assert.Equal(label, (await _wallet.GetSubaddress(1, 0)).GetLabel());
    }

    // Can create a subaddress with and without a label
    [Fact]
    public async Task TestCreateSubaddress()
    {
        // create subaddresses across accounts
        List<MoneroAccount> accounts = await _wallet.GetAccounts(true, false, null);
        if (accounts.Count < 2)
        {
            await _wallet.CreateAccount(null);
        }

        accounts = await _wallet.GetAccounts(true, false, null);
        Assert.True(accounts.Count > 1);
        for (uint accountIdx = 0; accountIdx < 2; accountIdx++)
        {
            // create a subaddress with no label
            List<MoneroSubaddress> subaddresses = await _wallet.GetSubaddresses(accountIdx, false, null);
            MoneroSubaddress subaddress = await _wallet.CreateSubaddress(accountIdx, null);
            Assert.Null(subaddress.GetLabel());
            TestSubaddress(subaddress);
            List<MoneroSubaddress> subaddressesNew = await _wallet.GetSubaddresses(accountIdx, false, null);
            Assert.Equal(subaddressesNew.Count - 1, subaddresses.Count);
            Assert.True(subaddress.Equals(subaddressesNew[subaddressesNew.Count - 1]));

            // create subaddress with label
            subaddresses = await _wallet.GetSubaddresses(accountIdx, false, null);
            string uuid = GenUtils.GetGuid();
            subaddress = await _wallet.CreateSubaddress(accountIdx, uuid);
            Assert.Equal(uuid, subaddress.GetLabel());
            TestSubaddress(subaddress);
            subaddressesNew = await _wallet.GetSubaddresses(accountIdx, false, null);
            Assert.Equal(subaddresses.Count, subaddressesNew.Count - 1);
            Assert.True(subaddress.Equals(subaddressesNew[subaddressesNew.Count - 1]));
        }
    }

    // Can get subaddresses at a specified account index
    [Fact]
    public async Task TestGetSubaddresses()
    {
        List<MoneroAccount> accounts = await _wallet.GetAccounts(true, false, null);
        Assert.NotEmpty(accounts);
        foreach (MoneroAccount account in accounts)
        {
            uint? accountIndex = account.GetIndex();

            if (accountIndex == null)
            {
                throw new MoneroError("Account index is null");
            }

            List<MoneroSubaddress> subaddresses = await _wallet.GetSubaddresses((uint)accountIndex, false, null);
            Assert.NotEmpty(subaddresses);
            foreach (MoneroSubaddress subaddress in subaddresses)
            {
                TestSubaddress(subaddress);
                Assert.Equal(account.GetIndex(), subaddress.GetAccountIndex());
            }
        }
    }

    // Can get subaddresses at specified account and subaddress indices
    [Fact]
    public async Task TestGetSubaddressesByIndices()
    {
        List<MoneroAccount> accounts = await _wallet.GetAccounts(false, true, null);
        Assert.NotEmpty(accounts);
        foreach (MoneroAccount account in accounts)
        {
            uint? accountIndex = account.GetIndex();
            if (accountIndex == null)
            {
                throw new MoneroError("Account index is null");
            }

            // get subaddresses
            List<MoneroSubaddress> subaddresses = await _wallet.GetSubaddresses((uint)accountIndex, true, null);
            Assert.True(subaddresses.Count > 0);
            // remove a subaddress for a query if possible
            if (subaddresses.Count > 1)
            {
                subaddresses.RemoveAt(0);
            }

            // get subaddress indices
            List<uint> subaddressIndices = new();
            foreach (MoneroSubaddress subaddress in subaddresses)
            {
                uint? subaddressIndex = subaddress.GetIndex();

                if (subaddressIndex == null)
                {
                    throw new MoneroError("Subaddress index is null");
                }

                subaddressIndices.Add((uint)subaddressIndex);
            }

            Assert.True(subaddressIndices.Count > 0);
            // fetch subaddresses by indices
            List<MoneroSubaddress> fetchedSubaddresses =
                await _wallet.GetSubaddresses((uint)accountIndex, true, subaddressIndices);

            // original subaddresses (minus one removed if applicable) is equal to fetched subaddresses
            int i = 0;

            foreach (MoneroSubaddress subaddr in subaddresses)
            {
                Assert.True(subaddr.Equals(fetchedSubaddresses[i]));
                i++;
            }
        }
    }

    // Can get a subaddress at a specified account and subaddress index
    [Fact]
    public async Task TestGetSubaddressByIndex()
    {
        List<MoneroAccount> accounts = await _wallet.GetAccounts(false, false, null);
        Assert.True(accounts.Count > 0);
        foreach (MoneroAccount account in accounts)
        {
            uint? accountIdx = account.GetIndex();

            if (accountIdx == null)
            {
                throw new MoneroError("Account index is null");
            }

            List<MoneroSubaddress> subaddresses = await _wallet.GetSubaddresses((uint)accountIdx, false, null);
            Assert.True(subaddresses.Count > 0);

            foreach (MoneroSubaddress subaddress in subaddresses)
            {
                TestSubaddress(subaddress);
                uint? subaddressIdx = subaddress.GetIndex();
                if (subaddressIdx == null) { throw new MoneroError("Subaddress index is null"); }

                Assert.True(subaddress.Equals(await _wallet.GetSubaddress((uint)accountIdx, (uint)subaddressIdx)));
                Assert.True(subaddress.Equals(
                    (await _wallet.GetSubaddresses((uint)accountIdx, false, [(uint)subaddressIdx]))
                    .First())); // test a plural call with a single subaddress number
            }
        }
    }

    // Can set subaddress labels
    [Fact]
    public async Task TestSetSubaddressLabel()
    {
        // create subaddresses
        while ((await _wallet.GetSubaddresses(0, true, null)).Count < 3)
        {
            await _wallet.CreateSubaddress(0, null);
        }

        uint subaddressesCount = (uint)(await _wallet.GetSubaddresses(0, true, null)).Count;
        // set subaddress labels
        for (uint subaddressIdx = 0; subaddressIdx < subaddressesCount; subaddressIdx++)
        {
            string label = GenUtils.GetGuid();
            await _wallet.SetAccountLabel(0, subaddressIdx, label);
            Assert.Equal(label, (await _wallet.GetSubaddress(0, subaddressIdx)).GetLabel());
        }
    }

    // Can sync (without progress)
    [Fact]
    public async Task TestSyncWithoutProgress()
    {
        ulong numBlocks = 100;
        ulong chainHeight = await _daemon.GetHeight();
        Assert.True(chainHeight >= numBlocks);
        MoneroSyncResult result = await _wallet.Sync(chainHeight - numBlocks, null); // sync end of a chain
        Assert.True(result.GetNumBlocksFetched() >= 0);
        Assert.NotNull(result.GetReceivedMoney());
    }

    // Can get the locked and unlocked balances of the wallet, accounts, and subaddresses
    [Fact]
    public async Task TestGetAllBalances()
    {
        // fetch accounts with all info as reference
        List<MoneroAccount> accounts = await _wallet.GetAccounts(true, false, null);

        // test that balances add up between accounts and wallet
        ulong accountsBalance = 0;
        ulong accountsUnlockedBalance = 0;
        foreach (MoneroAccount account in accounts)
        {
            accountsBalance += account.GetBalance();
            accountsUnlockedBalance += account.GetUnlockedBalance();

            // test that balances add up between subaddresses and accounts
            ulong subaddressesBalance = 0;
            ulong subaddressesUnlockedBalance = 0;
            foreach (MoneroSubaddress subaddress in account.GetSubaddresses()!)
            {
                subaddressesBalance += subaddress.GetBalance() ?? 0;
                subaddressesUnlockedBalance += subaddress.GetUnlockedBalance() ?? 0;

                // test that balances are consistent with getAccounts() call
                Assert.Equal((await _wallet.GetBalance(subaddress.GetAccountIndex(), subaddress.GetIndex())).ToString(),
                    subaddress.GetBalance().ToString());
                Assert.Equal(
                    (await _wallet.GetUnlockedBalance(subaddress.GetAccountIndex(), subaddress.GetIndex())).ToString(),
                    subaddress.GetUnlockedBalance().ToString());
            }

            Assert.Equal((await _wallet.GetBalance(account.GetIndex(), null)).ToString(),
                subaddressesBalance.ToString());
            Assert.Equal((await _wallet.GetUnlockedBalance(account.GetIndex(), null)).ToString(),
                subaddressesUnlockedBalance.ToString());
        }

        TestUtils.TestUnsignedBigInteger(accountsBalance);
        TestUtils.TestUnsignedBigInteger(accountsUnlockedBalance);
        Assert.Equal((await _wallet.GetBalance(null, null)).ToString(), accountsBalance.ToString());
        Assert.Equal((await _wallet.GetUnlockedBalance(null, null)).ToString(), accountsUnlockedBalance.ToString());
    }

    // Can save and close the wallet in a single call
    [Fact]
    public async Task TestSaveAndClose()
    {
        // create a random wallet
        string password = "";
        IMoneroWallet moneroWallet = await CreateWallet(new MoneroWalletConfig().SetPassword(password));
        string path = await moneroWallet.GetPath();

        // set an attribute
        string uuid = GenUtils.GetGuid();
        await moneroWallet.SetAttribute("id", uuid);

        // close the wallet without saving
        await CloseWallet(moneroWallet);

        // re-open the wallet and ensure the attribute was not saved
        moneroWallet = await OpenWallet(new MoneroWalletConfig().SetPath(path).SetPassword(password));
        Assert.Null(await moneroWallet.GetAttribute("id"));

        // set the attribute and close with saving
        await moneroWallet.SetAttribute("id", uuid);
        await CloseWallet(moneroWallet, true);

        // re-open the wallet and ensure the attribute was saved
        moneroWallet = await OpenWallet(new MoneroWalletConfig().SetPath(path).SetPassword(password));
        Assert.Equal(uuid, await moneroWallet.GetAttribute("id"));
        await CloseWallet(moneroWallet);
    }

    #region Test Utils

    private static void TestSubaddress(MoneroSubaddress? subaddress)
    {
        Assert.NotNull(subaddress);
        Assert.True(subaddress.GetAccountIndex() >= 0);
        Assert.True(subaddress.GetIndex() >= 0);
        Assert.NotNull(subaddress.GetAddress());
        string? label = subaddress.GetLabel();
        Assert.True(label == null || label.Length != 0);
        TestUtils.TestUnsignedBigInteger(subaddress.GetBalance());
        TestUtils.TestUnsignedBigInteger(subaddress.GetUnlockedBalance());
        Assert.True(subaddress.GetNumUnspentOutputs() >= 0);
        Assert.NotNull(subaddress.IsUsed());

        if (subaddress.GetBalance() > 0)
        {
            Assert.True(subaddress.IsUsed());
        }

        Assert.True(subaddress.GetNumBlocksToUnlock() >= 0);
    }

    private static void TestAccount(MoneroAccount? account)
    {
        // test account
        Assert.NotNull(account);
        uint? accountIndex = account.GetIndex();
        Assert.NotNull(accountIndex);
        MoneroUtils.ValidateAddress(account.GetPrimaryAddress(), TestUtils.NETWORK_TYPE);
        TestUtils.TestUnsignedBigInteger(account.GetBalance());
        TestUtils.TestUnsignedBigInteger(account.GetUnlockedBalance());

        // if given, test subaddresses and that their balances add up to account balances
        if (account.GetSubaddresses() != null)
        {
            ulong balance = 0;
            ulong unlockedBalance = 0;
            List<MoneroSubaddress> subaddresses = account.GetSubaddresses() ?? [];

            uint i = 0;

            foreach (MoneroSubaddress subaddress in subaddresses)
            {
                TestSubaddress(subaddress);
                Assert.Equal(accountIndex, subaddress.GetAccountIndex());
                Assert.Equal(i, subaddress.GetIndex());
                balance += subaddress.GetBalance() ?? 0;
                unlockedBalance += subaddress.GetUnlockedBalance() ?? 0;
                i++;
            }

            Assert.True(balance.Equals(account.GetBalance()),
                "Subaddress balances " + balance + " != account " + accountIndex + " balance " + account.GetBalance());
            Assert.True(unlockedBalance.Equals(account.GetUnlockedBalance()),
                "Subaddress unlocked balances " + unlockedBalance + " != account " + accountIndex +
                " unlocked balance " + account.GetUnlockedBalance());
        }

        // tag must be undefined or non-empty
        string? tag = account.GetTag();
        Assert.True(tag == null || tag.Length > 0);
    }

    #endregion
}