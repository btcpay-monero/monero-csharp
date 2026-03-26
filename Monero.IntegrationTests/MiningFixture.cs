using NUnit.Framework;

namespace Monero.IntegrationTests;

[SetUpFixture]
public class MiningFixture : MoneroIntegrationTestBase
{
    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        await MineToHeight(10);
    }

    [OneTimeTearDown]
    public async Task RunAfterAllTests()
    {
        await TestUtils.GetDaemonRpc().StopMining();
    }

    private static async Task MineToHeight(ulong targetHeight)
    {
        var daemon = TestUtils.GetDaemonRpc();

        var currentHeight = await daemon.GetHeight();
        if (currentHeight >= targetHeight) return;

        var miningStatus = await daemon.GetMiningStatus();
        if (miningStatus.IsActive != true)
        {
            await daemon.StartMining(
                "9xSyMy1r9h3BVjMrF3CTqQCQy36yCfkpn7uVfMyTUbez3hhumqBUqGUNNALjcd7f1HJBRdeH82bCC3veFHW7z3xm28gug4d",
                1,
                false,
                false
            );
        }
        while (true)
        {
            var height = await daemon.GetHeight();
            if (height >= targetHeight)
            {
                break;
            }
            await Task.Delay(1000);
        }
    }
}