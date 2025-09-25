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
                "42U9v3qs5CjZEePHBZHwuSckQXebuZu299NSmVEmQ41YJZQhKcPyujyMSzpDH4VMMVSBo3U3b54JaNvQLwAjqDhKS3rvM3L",
                numThreads,
                false,
                false
            );
    }
}