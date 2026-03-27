using Monero.Wallet.Common;

using NUnit.Framework;

namespace Monero.IntegrationTests;

[SetUpFixture]
public class MiningFixture : MoneroIntegrationTestBase
{
    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        await Wallet
            .CreateWallet(new MoneroWalletConfig()
                .SetPath(TestUtils.WalletName)
                .SetPassword(TestUtils.WalletPassword)
                .SetSeed(TestUtils.Seed)
                .SetRestoreHeight(0)
            );
        await MineToHeight(71);
        List<MoneroAccount> moneroAccounts = await Wallet.GetAccounts(true, false, null);
        foreach (MoneroAccount account in moneroAccounts)
        {
            await NUnit.Framework.TestContext.Progress.WriteLineAsync(
                $"Wallet's account with index {account.AccountIndex}: total balance {account.Balance}, unlocked balance {account.UnlockedBalance}");
        }
    }

    [OneTimeTearDown]
    public async Task RunAfterAllTests()
    {
        await TestUtils.GetDaemonRpc().StopMining();
    }

    private static async Task MineToHeight(ulong targetHeight)
    {
        var currentHeight = await Daemon.GetHeight();
        if (currentHeight >= targetHeight) return;

        var miningStatus = await Daemon.GetMiningStatus();
        if (miningStatus.IsActive != true)
        {
            await Daemon.StartMining(
                "9xSyMy1r9h3BVjMrF3CTqQCQy36yCfkpn7uVfMyTUbez3hhumqBUqGUNNALjcd7f1HJBRdeH82bCC3veFHW7z3xm28gug4d",
                1,
                false,
                false
            );
            await NUnit.Framework.TestContext.Progress.WriteLineAsync("Mining started.");
        }

        ulong lastLoggedHeight = currentHeight;
        while (true)
        {
            var height = await Daemon.GetHeight();
            if (height >= targetHeight)
            {
                break;
            }

            if (height != lastLoggedHeight)
            {
                lastLoggedHeight = height;
                await NUnit.Framework.TestContext.Progress.WriteLineAsync($"Current height: {height}/{targetHeight}");
            }

            await Task.Delay(1000);
        }

        await NUnit.Framework.TestContext.Progress.WriteLineAsync($"Mining to height {targetHeight} completed.");
    }
}