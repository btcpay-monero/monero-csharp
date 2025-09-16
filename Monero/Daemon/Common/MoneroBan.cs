namespace Monero.Daemon.Common;

public class MoneroBan
{
    private string? _host; // e.g. 192.168.1.100
    private uint? _ip; // integer formatted IP
    private bool? _isBanned;
    private ulong? _seconds;

    public string? GetHost()
    {
        return _host;
    }

    public MoneroBan SetHost(string? host)
    {
        this._host = host;
        return this;
    }

    public uint? GetIp()
    {
        return _ip;
    }

    public MoneroBan SetIp(uint? ip)
    {
        this._ip = ip;
        return this;
    }

    public bool? IsBanned()
    {
        return _isBanned;
    }

    public MoneroBan SetIsBanned(bool? isBanned)
    {
        this._isBanned = isBanned;
        return this;
    }

    public ulong? GetSeconds()
    {
        return _seconds;
    }

    public MoneroBan SetSeconds(ulong? seconds)
    {
        this._seconds = seconds;
        return this;
    }
}