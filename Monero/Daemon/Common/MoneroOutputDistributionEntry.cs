namespace Monero.Daemon.Common;

public class MoneroOutputDistributionEntry
{
    public ulong? Amount { get; set; }
    public uint? Base { get; set; }
    public List<uint>? Distribution { get; set; }
    public ulong? StartHeight { get; set; }
}