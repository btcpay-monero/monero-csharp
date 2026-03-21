namespace Monero.IntegrationTests.Utils;

public static class StartMining
{
    public static void Start()
    {
        Start(1);
    }

    private static void Start(ulong numThreads)
    {
        TestUtils
            .GetDaemonRpc()
            .StartMining(
                "9xSyMy1r9h3BVjMrF3CTqQCQy36yCfkpn7uVfMyTUbez3hhumqBUqGUNNALjcd7f1HJBRdeH82bCC3veFHW7z3xm28gug4d",
                numThreads,
                false,
                false
            );
    }
}