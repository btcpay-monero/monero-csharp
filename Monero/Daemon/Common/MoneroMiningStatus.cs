namespace Monero.Daemon.Common;

public class MoneroMiningStatus
{
    private string? _address;
    private bool? _isActive;
    private bool? _isBackground;
    private uint? _numThreads;
    private ulong? _speed;

    public bool? IsActive()
    {
        return _isActive;
    }

    public void SetIsActive(bool? isActive)
    {
        this._isActive = isActive;
    }

    public bool? IsBackground()
    {
        return _isBackground;
    }

    public void SetIsBackground(bool? isBackground)
    {
        this._isBackground = isBackground;
    }

    public string? GetAddress()
    {
        return _address;
    }

    public void SetAddress(string? address)
    {
        this._address = address;
    }

    public ulong? GetSpeed()
    {
        return _speed;
    }

    public void SetSpeed(ulong? speed)
    {
        this._speed = speed;
    }

    public uint? GetNumThreads()
    {
        return _numThreads;
    }

    public void SetNumThreads(uint? numThreads)
    {
        this._numThreads = numThreads;
    }
}