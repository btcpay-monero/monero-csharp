namespace Monero.IntegrationTests.Utils;

public static class StopMining
{
    public static async Task<bool> IsStopped()
    {
        return (await TestUtils.GetDaemonRpc().GetMiningStatus()).IsActive() != true;
    }

    public static async Task Stop()
    {
        if (await IsStopped())
        {
            return;
        }

        await TestUtils.GetDaemonRpc().StopMining();
    }
}