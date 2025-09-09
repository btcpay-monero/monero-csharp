
namespace Monero.Daemon.Common;

public class MoneroMiningStatus
{
    private bool? isActive;
    private bool? isBackground;
    private string? address;
    private ulong? speed;
    private uint? numThreads;

    public bool? IsActive()
    {
        return isActive;
    }

    public void SetIsActive(bool? isActive)
    {
        this.isActive = isActive;
    }

    public bool? IsBackground()
    {
        return isBackground;
    }

    public void SetIsBackground(bool? isBackground)
    {
        this.isBackground = isBackground;
    }

    public string? GetAddress()
    {
        return address;
    }

    public void SetAddress(string? address)
    {
        this.address = address;
    }

    public ulong? GetSpeed()
    {
        return speed;
    }

    public void SetSpeed(ulong? speed)
    {
        this.speed = speed;
    }

    public uint? GetNumThreads()
    {
        return numThreads;
    }

    public void SetNumThreads(uint? numThreads)
    {
        this.numThreads = numThreads;
    }
}