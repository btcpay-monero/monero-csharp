namespace Monero.Common;

public class MoneroVersion
{
    private bool? _isRelease;
    private long? _number;

    public MoneroVersion()
    {
    }

    public MoneroVersion(long? number)
    {
        _number = number;
    }

    public MoneroVersion(long? number, bool? isRelease)
    {
        _number = number;
        _isRelease = isRelease;
    }

    public long? GetNumber() { return _number; }
    public bool? IsRelease() { return _isRelease; }

    public MoneroVersion SetNumber(long? number)
    {
        _number = number;
        return this;
    }

    public MoneroVersion SetIsRelease(bool? isRelease)
    {
        _isRelease = isRelease;
        return this;
    }
}