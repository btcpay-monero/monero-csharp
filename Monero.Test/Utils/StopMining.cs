
namespace Monero.Test.Utils;

public static class StopMining
{
    public static void Stop()
    {
        TestUtils.GetDaemonRpc().StopMining();
    }
}