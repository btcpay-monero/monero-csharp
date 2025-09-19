namespace Monero.Common;

public class MoneroKeyImage
{
    public enum SpentStatus
    {
        NotSpent,
        Confirmed,
        TxPool
    }

    private string? _hex;
    private string? _signature;

    public MoneroKeyImage()
    {
    }

    public MoneroKeyImage(string? hex)
    {
        _hex = hex;
    }

    public MoneroKeyImage(string? hex, string? signature)
    {
        _hex = hex;
        _signature = signature;
    }

    public MoneroKeyImage(MoneroKeyImage keyImage)
    {
        _hex = keyImage._hex;
        _signature = keyImage._signature;
    }

    public MoneroKeyImage Clone()
    {
        return new MoneroKeyImage(this);
    }

    public static SpentStatus ParseStatus(int status)
    {
        if (status == 1)
        {
            return SpentStatus.NotSpent;
        }

        if (status == 2)
        {
            return SpentStatus.Confirmed;
        }

        if (status == 3)
        {
            return SpentStatus.TxPool;
        }

        throw new MoneroError("Invalid integer value for spent status: " + status);
    }

    public string? GetHex()
    {
        return _hex;
    }

    public MoneroKeyImage SetHex(string? hex)
    {
        _hex = hex;
        return this;
    }

    public string? GetSignature()
    {
        return _signature;
    }

    public MoneroKeyImage SetSignature(string? signature)
    {
        _signature = signature;
        return this;
    }

    public MoneroKeyImage Merge(MoneroKeyImage? keyImage)
    {
        throw new NotImplementedException("MoneroKeyImage.Merge(): not implemented");
    }
}