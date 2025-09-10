namespace Monero.Daemon.Common;

public class MoneroMiningStatus
{
    private string? address;
    private bool? isActive;
    private bool? isBackground;
    private uint? numThreads;
    private ulong? speed;

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