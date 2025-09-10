namespace Monero.Common;

public class MoneroVersion
{
    private bool? _isRelease;
    private int? _number;

    public MoneroVersion(int? number = null, bool? isRelease = null)
    {
        _number = number;
        _isRelease = isRelease;
    }

    public int? GetNumber() { return _number; }
    public bool? IsRelease() { return _isRelease; }

    public MoneroVersion SetNumber(int? number)
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