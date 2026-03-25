using Monero.Common;
using Monero.IntegrationTests.Utils;
using Monero.Wallet;
using Monero.Wallet.Common;

using NUnit.Framework;

namespace Monero.IntegrationTests;

public class MoneroWalletRpcIntegrationTest : MoneroIntegrationTestBase
{
    private async Task<IMoneroWallet> OpenWallet(MoneroWalletConfig? config)
    {
        // assign defaults
        if (config == null)
        {
            config = new MoneroWalletConfig();
        }

        if (config.GetPassword() == null)
        {
            config.SetPassword(TestUtils.WalletPassword);
        }

        if (config.GetServer() == null)
        {
            config.SetServer(Daemon.GetRpcConnection());
        }

        // create a client connected to an internal monero-wallet-rpc process
        MoneroWalletRpc moneroWalletRpc = await TestUtils.GetCreateWallet();

        // open wallet
        await moneroWalletRpc.OpenWallet(config);
        await moneroWalletRpc.SetDaemonConnection(await moneroWalletRpc.GetDaemonConnection(), true, null);
        if (await moneroWalletRpc.IsConnectedToDaemon())
        {
            await moneroWalletRpc.StartSyncing(TestUtils.SyncPeriodInMs);
        }

        return moneroWalletRpc;
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
            config.SetPassword(TestUtils.WalletPassword);
        }

        if (config.GetRestoreHeight() == null && !random)
        {
            config.SetRestoreHeight(0);
        }

        if (config.GetServer() == null)
        {
            config.SetServer(Daemon.GetRpcConnection());
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
                await walletRpc.StartSyncing(TestUtils.SyncPeriodInMs);
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

    private static async Task CloseWallet(IMoneroWallet walletInstance) { await CloseWallet(walletInstance, false); }

    private static async Task CloseWallet(IMoneroWallet walletInstance, bool save)
    {
        MoneroWalletRpc walletRpc = (MoneroWalletRpc)walletInstance;
        string walletPath = await walletRpc.GetPath();

        if (string.IsNullOrEmpty(walletPath))
        {
            return;
        }

        await walletRpc.Close(save);
    }

    private async Task TestWallet(Func<Task> action, IMoneroWallet moneroWallet)
    {
        await TestWallet(action, moneroWallet, false);
    }

    private static async Task TestWallet(Func<Task> action, IMoneroWallet moneroWallet, bool checkSeed)
    {
        await action();

        if (checkSeed)
        {
            MoneroUtils.ValidateMnemonic(await moneroWallet.GetSeed());
        }
    }

    // Can create a random wallet
    [Test]
    public async Task TestCreateWalletRandom()
    {
        try
        {
            // create a random wallet
            IMoneroWallet moneroWallet = await CreateWallet(new MoneroWalletConfig());
            string path = await moneroWallet.GetPath();

            await TestWallet(async () =>
            {
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
            catch (Exception ex)
            {
                Assert.That(ex.Message, Is.EqualTo("Wallet already exists: " + path));
            }

            // attempt to create a wallet with an unknown language
            try
            {
                await CreateWallet(new MoneroWalletConfig().SetLanguage("english")); // TODO: support lowercase?
                throw new Exception("Should have thrown error");
            }
            catch (Exception ex)
            {
                Assert.That(ex.Message, Is.EqualTo("Unknown language: english"));
            }
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    // Can create a wallet from a seed.
    [Test]
    public async Task TestCreateWalletFromSeed()
    {
        // save for comparison
        string primaryAddress = await Wallet.GetPrimaryAddress();
        string privateViewKey = await Wallet.GetPrivateViewKey();
        string privateSpendKey = await Wallet.GetPrivateSpendKey();

        // recreate the test wallet from seed
        IMoneroWallet moneroWallet = await CreateWallet(new MoneroWalletConfig().SetSeed(TestUtils.Seed)
            .SetRestoreHeight(TestUtils.FirstReceiveHeight));
        string path = await moneroWallet.GetPath();

        await TestWallet(async () =>
        {
            Assert.That(await moneroWallet.GetPrimaryAddress(), Is.EqualTo(primaryAddress));
            Assert.That(await moneroWallet.GetPrivateViewKey(), Is.EqualTo(privateViewKey));
            Assert.That(await moneroWallet.GetPrivateSpendKey(), Is.EqualTo(privateSpendKey));
            Assert.That(await moneroWallet.GetSeed(), Is.EqualTo(TestUtils.Seed));
        }, moneroWallet);

        // attempt to create a wallet with two missing words
        try
        {
            const string invalidMnemonic =
                "memoir desk algebra inbound innocent unplugs fully okay five inflamed giant factual ritual toyed topic snake unhappy guarded tweezers haunted inundate giant";
            await CreateWallet(new MoneroWalletConfig().SetSeed(invalidMnemonic)
                .SetRestoreHeight(TestUtils.FirstReceiveHeight));
        }
        catch (Exception ex)
        {
            Assert.That(ex.Message, Is.EqualTo("Invalid mnemonic"));
        }

        await AttemptToCreateWalletAtSamePath(path);
    }

    private async Task AttemptToCreateWalletAtSamePath(string path)
    {
        try
        {
            await CreateWallet(new MoneroWalletConfig().SetPath(path));
            throw new Exception("Should have thrown error");
        }
        catch (Exception ex)
        {
            Assert.That(ex.Message.ToLower(), Does.Contain("already exists"));
        }
    }

    // Can create a wallet from a seed with a seed offset
    [Test]
    public async Task TestCreateWalletFromSeedWithOffset()
    {
        try
        {
            // create a test wallet with offset
            IMoneroWallet moneroWallet = await CreateWallet(new MoneroWalletConfig().SetSeed(TestUtils.Seed)
                .SetRestoreHeight(TestUtils.FirstReceiveHeight).SetSeedOffset("my secret offset!"));

            await TestWallet(async () =>
            {
                MoneroUtils.ValidateMnemonic(await moneroWallet.GetSeed());
                Assert.That(await moneroWallet.GetSeed(), Is.Not.EqualTo(TestUtils.Seed));
                Assert.That(await moneroWallet.GetPrimaryAddress(), Is.Not.EqualTo(TestUtils.Address));
            }, moneroWallet);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    // Can create a wallet from keys
    [Test]
    public async Task TestCreateWalletFromKeys()
    {
        // save for comparison
        string primaryAddress = await Wallet.GetPrimaryAddress();
        string privateViewKey = await Wallet.GetPrivateViewKey();
        string privateSpendKey = await Wallet.GetPrivateSpendKey();

        // recreate the test wallet from keys
        IMoneroWallet moneroWallet = await CreateWallet(new MoneroWalletConfig().SetPrimaryAddress(primaryAddress)
            .SetPrivateViewKey(privateViewKey).SetPrivateSpendKey(privateSpendKey)
            .SetRestoreHeight(await Daemon.GetHeight()));
        string path = await moneroWallet.GetPath();

        Func<Task> action = async () =>
        {
            Assert.That(await moneroWallet.GetPrimaryAddress(), Is.EqualTo(primaryAddress));
            Assert.That(await moneroWallet.GetPrivateViewKey(), Is.EqualTo(privateViewKey));
            Assert.That(await moneroWallet.GetPrivateSpendKey(), Is.EqualTo(privateSpendKey));
            if (TestUtils.TestsInContainer)
            {
                Assert.That(await moneroWallet.IsConnectedToDaemon(), Is.True,
                    "Wallet created from keys is not connected to authenticated daemon");
            }
        };

        await TestWallet(action, moneroWallet, false);

        await AttemptToCreateWalletAtSamePath(path);
    }

    // Can get the wallet's version
    [Test]
    public async Task TestGetVersion()
    {
        MoneroVersion version = await Wallet.GetVersion();
        Assert.That(version.Number, Is.Not.Null);
        Assert.That(version.Number, Is.GreaterThan(0));
        Assert.That(version.IsRelease, Is.Not.Null);
    }

    // Can get the wallet's path
    [Test]
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
        Assert.That(await moneroWallet.GetAttribute("uuid"), Is.EqualTo(uuid));
        await CloseWallet(moneroWallet);
    }

    // Can get the seed
    [Test]
    public async Task TestGetSeed()
    {
        string seed = await Wallet.GetSeed();
        MoneroUtils.ValidateMnemonic(seed);
        Assert.That(seed, Is.EqualTo(TestUtils.Seed));
    }

    // Can get the language of the seed
    [Test, Ignore("monero-wallet-rpc does not support getting seed language")]
    public async Task TestGetSeedLanguage()
    {
        string language = await Wallet.GetSeedLanguage();
        Assert.That(language, Is.EqualTo(IMoneroWallet.DefaultLanguage));
    }

    // Can get the private view key
    [Test]
    public async Task TestGetPrivateViewKey()
    {
        string privateViewKey = await Wallet.GetPrivateViewKey();
        MoneroUtils.ValidatePrivateViewKey(privateViewKey);
    }

    // Can get the private spent key
    [Test]
    public async Task TestGetPrivateSpendKey()
    {
        string privateSpendKey = await Wallet.GetPrivateSpendKey();
        MoneroUtils.ValidatePrivateSpendKey(privateSpendKey);
    }

    // Can get the public view key
    [Test, Ignore("Enable after monero-project fix (https://github.com/monero-project/monero/pull/9364)")]
    public async Task TestGetPublicViewKey()
    {
        string publicViewKey = await Wallet.GetPublicViewKey();
        MoneroUtils.ValidatePrivateSpendKey(publicViewKey);
    }

    // Can get the public view key
    [Test, Ignore("Enable after monero-project fix (https://github.com/monero-project/monero/pull/9364)")]
    public async Task TestGetPublicSpendKey()
    {
        string publicSpendKey = await Wallet.GetPublicSpendKey();
        MoneroUtils.ValidatePrivateSpendKey(publicSpendKey);
    }

    // Can get the primary address
    [Test]
    public async Task TestGetPrimaryAddress()
    {
        Assert.That(await Wallet.GetAddress(0, 0), Is.EqualTo(await Wallet.GetPrimaryAddress()));
    }

    // Can get the address of a subaddress at a specified account and subaddress index
    [Test]
    public async Task TestGetSubaddressAddress()
    {
        Assert.That(await Wallet.GetPrimaryAddress(), Is.EqualTo((await Wallet.GetSubaddress(0, 0)).GetAddress()));
        foreach (MoneroAccount account in await Wallet.GetAccounts(true, true, null))
        {
            foreach (MoneroSubaddress subaddress in account.Subaddresses!)
            {
                Assert.That(subaddress.GetAddress(),
                    Is.EqualTo(await Wallet.GetAddress((uint)account.AccountIndex!, subaddress.Index)));
            }
        }
    }

    // Can get addresses out of range of used accounts and subaddresses
    [Test]
    public async Task TestGetSubaddressAddressOutOfRange()
    {
        List<MoneroAccount> accounts = await Wallet.GetAccounts(true, true, null);
        int accountIdx = accounts.Count - 1;
        MoneroAccount account = accounts[accountIdx];
        List<MoneroSubaddress>? subaddresses = account.Subaddresses;

        if (subaddresses == null)
        {
            throw new MoneroError("Subaddresses is null");
        }

        int subaddressIdx = subaddresses.Count;
        string? address = await Wallet.GetAddress((uint)accountIdx, (uint)subaddressIdx);
        Assert.That(address, Is.Null);
    }


    // Can get the current height that the wallet is synchronized to
    [Test]
    public async Task TestGetHeight()
    {
        ulong lastHeight = await Wallet.GetHeight();
        await Daemon.WaitForNextBlockHeader();

        const int maxRetries = 15;
        const int delayMs = 1000;
        bool blockFetched = false;
        ulong currentHeight = lastHeight;

        for (int i = 0; i < maxRetries; i++)
        {
            var syncResult = await Wallet.Sync(null, null);
            currentHeight = await Wallet.GetHeight();

            if (syncResult.NumBlocksFetched > 0 && currentHeight > lastHeight)
            {
                blockFetched = true;
                break;
            }

            await GenUtils.WaitForAsync(delayMs, NUnit.Framework.TestContext.CurrentContext.CancellationToken);
        }

        Assert.That(blockFetched, Is.True,
            $"No blocks fetched after {maxRetries}s. Last height: {lastHeight}, current height: {currentHeight}");
        Assert.That(currentHeight, Is.GreaterThan(lastHeight),
            $"Expected currentHeight: {currentHeight} > lastHeight: {lastHeight}");
    }

    // Can create a new account without a label
    [Test]
    public async Task TestCreateAccountWithoutLabel()
    {
        List<MoneroAccount> accountsBefore = await Wallet.GetAccounts(false, true, null);
        MoneroAccount createdAccount = await Wallet.CreateAccount(null);
        TestAccount(createdAccount);
        List<MoneroAccount> accountsAfter = await Wallet.GetAccounts(false, true, null);
        Assert.That(accountsAfter.Count, Is.EqualTo(accountsBefore.Count + 1));
    }

    // Can get accounts without subaddresses
    [Test]
    public async Task TestGetAccountsWithoutSubaddresses()
    {
        List<MoneroAccount> accounts = await Wallet.GetAccounts(false, true, null);
        Assert.That(accounts, Is.Not.Empty);
        foreach (MoneroAccount account in accounts)
        {
            TestAccount(account);
            Assert.That(account.Subaddresses, Is.Null);
        }
    }

    // Can get accounts with subaddresses
    [Test]
    public async Task TestGetAccountsWithSubaddresses()
    {
        List<MoneroAccount> accounts = await Wallet.GetAccounts(true, false, null);
        Assert.That(accounts, Is.Not.Empty);
        foreach (MoneroAccount account in accounts)
        {
            TestAccount(account);
            List<MoneroSubaddress> subaddresses = account.Subaddresses ?? [];
            Assert.That(subaddresses, Is.Not.Empty);
        }
    }

    // Can set account labels
    [Test]
    public async Task TestSetAccountLabel()
    {
        // create an account
        if ((await Wallet.GetAccounts(false, true, null)).Count < 2)
        {
            // create an account
            if ((await Wallet.GetAccounts(false, true, null)).Count < 2)
            {
                await Wallet.CreateAccount(null);
            }
        }

        // set account label
        string label = GenUtils.GetGuid();
        await Wallet.SetAccountLabel(1, 0, label);
        MoneroSubaddress subaddress = await Wallet.GetSubaddress(1, 0);
        Assert.That(subaddress.GetLabel(), Is.EqualTo(label));
    }

    // Can create an address with and without a label
    [Test]
    public async Task TestCreateAddress()
    {
        // create addresses across accounts
        List<MoneroAccount> accounts = await Wallet.GetAccounts(true, false, null);
        if (accounts.Count < 2)
        {
            await Wallet.CreateAccount(null);
        }

        accounts = await Wallet.GetAccounts(true, false, null);
        Assert.That(accounts.Count, Is.GreaterThan(1));
        for (uint accountIdx = 0; accountIdx < 2; accountIdx++)
        {
            // create an address with no label
            List<MoneroSubaddress> subaddresses = await Wallet.GetSubaddresses(accountIdx, false, null);
            MoneroSubaddress subaddress = (await Wallet.CreateAddress(accountIdx, null))[0];
            Assert.That(subaddress.GetLabel(), Is.EqualTo(string.Empty));
            TestSubaddress(subaddress);
            List<MoneroSubaddress> subaddressesNew = await Wallet.GetSubaddresses(accountIdx, false, null);
            Assert.That(subaddressesNew.Count - 1, Is.EqualTo(subaddresses.Count));
            Assert.That(subaddress.Equals(subaddressesNew[^1]), Is.True);

            // create an address with a label
            subaddresses = await Wallet.GetSubaddresses(accountIdx, false, null);
            string uuid = GenUtils.GetGuid();
            subaddress = (await Wallet.CreateAddress(accountIdx, uuid))[0];
            Assert.That(subaddress.GetLabel(), Is.EqualTo(uuid));
            TestSubaddress(subaddress);
            subaddressesNew = await Wallet.GetSubaddresses(accountIdx, false, null);
            Assert.That(subaddressesNew.Count - 1, Is.EqualTo(subaddresses.Count));
            Assert.That(subaddress.Equals(subaddressesNew[^1]), Is.True);
        }
    }

    // Can create multiple subaddresses at once
    [Test]
    public async Task TestCreateAddressMultiple()
    {
        const uint count = 3;
        List<MoneroSubaddress> subaddressesBeforeCreate = await Wallet.GetSubaddresses(0, false, null);
        List<MoneroSubaddress> created = await Wallet.CreateAddress(0, null, count);
        Assert.That(created.Count, Is.EqualTo((int)count));

        foreach (MoneroSubaddress subaddress in created)
        {
            var address = subaddress.GetAddress();
            Assert.That(subaddress.AccountIndex, Is.EqualTo(0u));
            Assert.That(address, Is.Not.Null.And.Not.Empty);
            TestSubaddress(subaddress);
        }

        List<MoneroSubaddress> subaddressesAfterCreate = await Wallet.GetSubaddresses(0, false, null);
        Assert.That(subaddressesAfterCreate.Count, Is.EqualTo(subaddressesBeforeCreate.Count + count));
    }

    // Can get subaddresses at a specified account index
    [Test]
    public async Task TestGetSubaddresses()
    {
        List<MoneroAccount> accounts = await Wallet.GetAccounts(true, false, null);
        Assert.That(accounts, Is.Not.Empty);
        foreach (MoneroAccount account in accounts)
        {
            uint? accountIndex = account.AccountIndex;

            if (accountIndex == null)
            {
                throw new MoneroError("Account index is null");
            }

            List<MoneroSubaddress> subaddresses = await Wallet.GetSubaddresses((uint)accountIndex, false, null);
            Assert.That(subaddresses, Is.Not.Empty);
            foreach (MoneroSubaddress subaddress in subaddresses)
            {
                TestSubaddress(subaddress);
                Assert.That(subaddress.AccountIndex, Is.EqualTo(account.AccountIndex));
            }
        }
    }

    // Can get subaddresses at a specified account and subaddress indices
    [Test]
    public async Task TestGetSubaddressesByIndices()
    {
        List<MoneroAccount> accounts = await Wallet.GetAccounts(false, true, null);
        Assert.That(accounts, Is.Not.Empty);
        foreach (var accountIndex in accounts.Select(account => account.AccountIndex))
        {
            if (accountIndex == null)
            {
                throw new MoneroError("Account index is null");
            }

            // get subaddresses
            List<MoneroSubaddress> subaddresses = await Wallet.GetSubaddresses((uint)accountIndex, true, null);
            Assert.That(subaddresses.Count, Is.GreaterThan(0));
            // remove a subaddress for a query if possible
            if (subaddresses.Count > 1)
            {
                subaddresses.RemoveAt(0);
            }

            // get subaddress indices
            List<uint> subaddressIndices = [];
            subaddressIndices.AddRange(subaddresses.Select(subaddress => subaddress.Index));

            Assert.That(subaddressIndices.Count, Is.GreaterThan(0));
            // fetch subaddresses by indices
            List<MoneroSubaddress> fetchedSubaddresses =
                await Wallet.GetSubaddresses((uint)accountIndex, true, subaddressIndices);

            // original subaddresses (minus one removed if applicable) is equal to fetched subaddresses
            for (int i = 0; i < subaddresses.Count; i++)
            {
                Assert.That(subaddresses[i].Equals(fetchedSubaddresses[i]), Is.True);
            }
        }
    }

    // Can get a subaddress at a specified account and subaddress index
    [Test]
    public async Task TestGetSubaddressByIndex()
    {
        List<MoneroAccount> accounts = await Wallet.GetAccounts(false, false, null);
        Assert.That(accounts.Count, Is.GreaterThan(0));
        foreach (var accountIdx in accounts.Select(account => account.AccountIndex))
        {
            if (accountIdx == null)
            {
                throw new MoneroError("Account index is null");
            }

            List<MoneroSubaddress> subaddresses = await Wallet.GetSubaddresses((uint)accountIdx, false, null);
            Assert.That(subaddresses.Count, Is.GreaterThan(0));

            foreach (MoneroSubaddress subaddress in subaddresses)
            {
                TestSubaddress(subaddress);
                uint? subaddressIdx = subaddress.Index;
                if (subaddressIdx == null) { throw new MoneroError("Subaddress index is null"); }

                Assert.That(
                    subaddress.Equals(await Wallet.GetSubaddress((uint)accountIdx, (uint)subaddressIdx)),
                    Is.True
                );
                Assert.That(
                    subaddress.Equals(
                        (await Wallet.GetSubaddresses((uint)accountIdx, false, [(uint)subaddressIdx]))
                        .First()
                    ),
                    Is.True
                ); // test a plural call with a single subaddress number
            }
        }
    }

    // Can set subaddress labels
    [Test]
    public async Task TestSetSubaddressLabel()
    {
        // create subaddresses
        while ((await Wallet.GetSubaddresses(0, true, null)).Count < 3)
        {
            await Wallet.CreateAddress(0, null);
        }

        uint subaddressesCount = (uint)(await Wallet.GetSubaddresses(0, true, null)).Count;
        // set subaddress labels
        for (uint subaddressIdx = 0; subaddressIdx < subaddressesCount; subaddressIdx++)
        {
            string label = GenUtils.GetGuid();
            await Wallet.SetAccountLabel(0, subaddressIdx, label);
            Assert.That((await Wallet.GetSubaddress(0, subaddressIdx)).GetLabel(), Is.EqualTo(label));
        }
    }

    // Can sync (without progress)
    [Test]
    public async Task TestSyncWithoutProgress()
    {
        ulong numBlocks = 10;
        ulong chainHeight = await Daemon.GetHeight();
        Assert.That(chainHeight, Is.GreaterThanOrEqualTo(numBlocks));
        MoneroSyncResponse response = await Wallet.Sync(chainHeight - numBlocks, null); // sync end of a chain
        Assert.That(response.NumBlocksFetched, Is.GreaterThanOrEqualTo(0));
        Assert.That(response.ReceivedMoney, Is.Not.Null);
    }

    // Can get the locked and unlocked balances of the wallet, accounts, and subaddresses
    [Test]
    public async Task TestGetAllBalances()
    {
        // fetch accounts with all info as reference
        List<MoneroAccount> accounts = await Wallet.GetAccounts(true, false, null);

        // test that balances add up between accounts and wallet
        ulong accountsBalance = 0;
        ulong accountsUnlockedBalance = 0;
        foreach (MoneroAccount account in accounts)
        {
            accountsBalance += account.Balance;
            accountsUnlockedBalance += account.UnlockedBalance;

            // test that balances add up between subaddresses and accounts
            ulong subaddressesBalance = 0;
            ulong subaddressesUnlockedBalance = 0;
            foreach (MoneroSubaddress subaddress in account.Subaddresses!)
            {
                subaddressesBalance += subaddress.GetBalance() ?? 0;
                subaddressesUnlockedBalance += subaddress.GetUnlockedBalance() ?? 0;

                // test that balances are consistent with getAccounts() call
                Assert.That(
                    (await Wallet.GetBalance(subaddress.AccountIndex, subaddress.Index)).ToString(),
                    Is.EqualTo(subaddress.GetBalance().ToString())
                );
                Assert.That(
                    (await Wallet.GetUnlockedBalance(subaddress.AccountIndex, subaddress.Index)).ToString(),
                    Is.EqualTo(subaddress.GetUnlockedBalance().ToString())
                );
            }

            Assert.That(
                (await Wallet.GetBalance(account.AccountIndex, null)).ToString(),
                Is.EqualTo(subaddressesBalance.ToString())
            );
            Assert.That(
                (await Wallet.GetUnlockedBalance(account.AccountIndex, null)).ToString(),
                Is.EqualTo(subaddressesUnlockedBalance.ToString())
            );
        }

        TestUtils.TestUnsignedBigInteger(accountsBalance);
        TestUtils.TestUnsignedBigInteger(accountsUnlockedBalance);
        Assert.That((await Wallet.GetBalance(null, null)).ToString(), Is.EqualTo(accountsBalance.ToString()));
        Assert.That((await Wallet.GetUnlockedBalance(null, null)).ToString(),
            Is.EqualTo(accountsUnlockedBalance.ToString()));
    }

    // Can save and close the wallet in a single call
    [Test]
    public async Task TestSaveAndClose()
    {
        // create a random wallet
        const string password = "";
        IMoneroWallet moneroWallet = await CreateWallet(new MoneroWalletConfig().SetPassword(password));
        string path = await moneroWallet.GetPath();

        // set an attribute
        string uuid = GenUtils.GetGuid();
        await moneroWallet.SetAttribute("id", uuid);

        // close the wallet without saving
        await CloseWallet(moneroWallet);

        // re-open the wallet and ensure the attribute was not saved
        moneroWallet = await OpenWallet(new MoneroWalletConfig().SetPath(path).SetPassword(password));
        Assert.That(await moneroWallet.GetAttribute("id"), Is.Null);

        // set the attribute and close with saving
        await moneroWallet.SetAttribute("id", uuid);
        await CloseWallet(moneroWallet, true);

        // re-open the wallet and ensure the attribute was saved
        moneroWallet = await OpenWallet(new MoneroWalletConfig().SetPath(path).SetPassword(password));
        Assert.That(await moneroWallet.GetAttribute("id"), Is.EqualTo(uuid));
        await CloseWallet(moneroWallet);
    }

    // Can validate address
    [Test]
    public async Task TestValidateAddress()
    {
        // mainnet primary and subaddress validation
        Assert.That(await Wallet.IsValidAddress(
            "42U9v3qs5CjZEePHBZHwuSckQXebuZu299NSmVEmQ41YJZQhKcPyujyMSzpDH4VMMVSBo3U3b54JaNvQLwAjqDhKS3rvM3L",
            false, true), Is.True);
        Assert.That(await Wallet.IsValidAddress(
            "891TQPrWshJVpnBR4ZMhHiHpLx1PUnMqa3ccV5TJFBbqcJa3DWhjBh2QByCv3Su7WDPTGMHmCKkiVFN2fyGJKwbM1t6G7Ea",
            true, true), Is.True);

        // testnet primary and subaddress validation
        Assert.That(await Wallet.IsValidAddress(
            "9tUBnNCkC3UKGygHCwYvAB1FscpjUuq5e9MYJd2rXuiiTjjfVeSVjnbSG5VTnJgBgy9Y7GTLfxpZNMUwNZjGfdFr1z79eV1",
            false, false), Is.True);
        Assert.That(await Wallet.IsValidAddress(
            "BgnKzHPJQDcg7xiP7bMN9MfPv9Z8ciT71iEMYnCdgBRBFETWgu9nKTr8fnzyGfU9h9gyNA8SFzYYzHfTS9KhqytSU943Nu1",
            true, false), Is.True);

        // stagenet primary and subaddress validation
        Assert.That(await Wallet.IsValidAddress(
            "5B8s3obCY2ETeQB3GNAGPK2zRGen5UeW1WzegSizVsmf6z5NvM2GLoN6zzk1vHyzGAAfA8pGhuYAeCFZjHAp59jRVQkunGS",
            false, true), Is.True);
        Assert.That(await Wallet.IsValidAddress(
            "778B5D2JmMh5TJVWFbygJR15dvio5Z5B24hfSrWDzeroM8j8Lqc9sMoFE6324xg2ReaAZqHJkgfGFRugRmYHugHZ4f17Gxo",
            true, true), Is.True);
    }

    #region Test Utils

    private static void TestSubaddress(MoneroSubaddress? subaddress)
    {
        Assert.That(subaddress, Is.Not.Null);
        Assert.That(subaddress!.AccountIndex, Is.GreaterThanOrEqualTo(0));
        Assert.That(subaddress.Index, Is.GreaterThanOrEqualTo(0));
        Assert.That(subaddress.GetAddress(), Is.Not.Null);
        TestUtils.TestUnsignedBigInteger(subaddress.GetBalance());
        TestUtils.TestUnsignedBigInteger(subaddress.GetUnlockedBalance());
        Assert.That(subaddress.GetNumUnspentOutputs(), Is.GreaterThanOrEqualTo(0));
        Assert.That(subaddress.IsUsed(), Is.Not.Null);

        if (subaddress.GetBalance() > 0)
        {
            Assert.That(subaddress.IsUsed(), Is.True);
        }

        Assert.That(subaddress.GetNumBlocksToUnlock(), Is.GreaterThanOrEqualTo(0));
    }

    private static void TestAccount(MoneroAccount? account)
    {
        // test account
        Assert.That(account, Is.Not.Null);
        uint? accountIndex = account!.AccountIndex;
        Assert.That(accountIndex, Is.Not.Null);
        TestUtils.TestUnsignedBigInteger(account.Balance);
        TestUtils.TestUnsignedBigInteger(account.UnlockedBalance);

        // if given, test subaddresses and that their balances add up to account balances
        if (account.Subaddresses != null)
        {
            ulong balance = 0;
            ulong unlockedBalance = 0;
            List<MoneroSubaddress> subaddresses = account.Subaddresses ?? [];

            uint i = 0;

            foreach (MoneroSubaddress subaddress in subaddresses)
            {
                TestSubaddress(subaddress);
                Assert.That(subaddress.AccountIndex, Is.EqualTo(accountIndex));
                Assert.That(subaddress.Index, Is.EqualTo(i));
                balance += subaddress.GetBalance() ?? 0;
                unlockedBalance += subaddress.GetUnlockedBalance() ?? 0;
                i++;
            }

            Assert.That(balance, Is.EqualTo(account.Balance),
                $"Subaddress balances {balance} != account {accountIndex} balance {account.Balance}");
            Assert.That(unlockedBalance, Is.EqualTo(account.UnlockedBalance),
                $"Subaddress unlocked balances {unlockedBalance} != account {accountIndex} unlocked balance {account.UnlockedBalance}");
        }
    }

    #endregion
}