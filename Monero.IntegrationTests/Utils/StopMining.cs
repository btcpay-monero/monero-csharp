namespace Monero.IntegrationTests.Utils;

public static class StopMining
{
    public static void Stop()
    {
        TestUtils.GetDaemonRpc().StopMining();
    }
}