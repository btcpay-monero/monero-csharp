using System.Text;

using Monero.Common;

namespace Monero.Wallet.Common;

public class MoneroDestination
{
    private string? _address;
    private ulong? _amount;

    public MoneroDestination()
    {
    }

    public MoneroDestination(string? address)
    {
        _address = address;
    }

    public MoneroDestination(string? address, ulong? amount)
    {
        _address = address;
        _amount = amount;
    }

    public MoneroDestination(MoneroDestination destination)
    {
        _address = destination._address;
        _amount = destination._amount;
    }

    public MoneroDestination Clone() { return new MoneroDestination(this); }

    public string? GetAddress()
    {
        return _address;
    }

    public MoneroDestination SetAddress(string? address)
    {
        _address = address;
        return this;
    }

    public ulong? GetAmount()
    {
        return _amount;
    }

    public MoneroDestination SetAmount(ulong? amount)
    {
        _amount = amount;
        return this;
    }

    public bool Equals(MoneroDestination? other)
    {
        if (other == null)
        {
            return false;
        }

        if (this == other)
        {
            return true;
        }

        return _address == other._address && _amount == other._amount;
    }

    public string ToString(int indent)
    {
        var sb = new StringBuilder();
        sb.Append(GenUtils.KvLine("Address", GetAddress(), indent));
        sb.Append(GenUtils.KvLine("Amount", GetAmount() != null ? GetAmount().ToString() : null, indent));
        string str = sb.ToString();
        return str.Substring(0, str.Length - 1);
    }
}