namespace Monero.Daemon.Common;

public class MoneroBan
{
    private string? host; // e.g. 192.168.1.100
    private uint? ip; // integer formatted IP
    private bool? isBanned;
    private ulong? seconds;

    public string? GetHost()
    {
        return host;
    }

    public MoneroBan SetHost(string? host)
    {
        this.host = host;
        return this;
    }

    public uint? GetIp()
    {
        return ip;
    }

    public MoneroBan SetIp(uint? ip)
    {
        this.ip = ip;
        return this;
    }

    public bool? IsBanned()
    {
        return isBanned;
    }

    public MoneroBan SetIsBanned(bool? isBanned)
    {
        this.isBanned = isBanned;
        return this;
    }

    public ulong? GetSeconds()
    {
        return seconds;
    }

    public MoneroBan SetSeconds(ulong? seconds)
    {
        this.seconds = seconds;
        return this;
    }
}