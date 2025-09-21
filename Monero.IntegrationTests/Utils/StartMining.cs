namespace Monero.IntegrationTests.Utils;

public static class StartMining
{
    public static async Task Start()
    {
        await Start(1);
    }

    public static async Task Start(string? address)
    {
        await Start(1, address);
    }

    public static async Task Start(ulong numThreads)
    {
        await Start(numThreads, null);
    }

    public static async Task<bool> IsMining()
    {
        return (await TestUtils.GetDaemonRpc().GetMiningStatus()).IsActive() == true;
    }

    private static async Task Start(ulong numThreads, string? address)
    {
        if (await IsMining())
        {
            return;
        }

        string miningAddress = string.IsNullOrEmpty(address) ? TestUtils.GetMiningAddress() : address;
        await TestUtils.GetDaemonRpc().StartMining(miningAddress, numThreads, false, false); // testnet
    }
}