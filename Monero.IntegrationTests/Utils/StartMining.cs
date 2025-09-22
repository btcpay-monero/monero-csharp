namespace Monero.IntegrationTests.Utils;

public static class StartMining
{
    public static void Start()
    {
        Start(1);
    }

    private static void Start(ulong numThreads)
    {
        TestUtils.GetDaemonRpc().StartMining(TestUtils.GetMiningAddress(), numThreads, false, false); // testnet
    }
}