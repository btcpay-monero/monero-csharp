using System.Text;

using Monero.Common;

namespace Monero.Wallet.Common;

public class MoneroIncomingTransfer : MoneroTransfer
{
    private string? _address;
    private ulong? _numSuggestedConfirmations;
    private uint? _subaddressIndex;

    public MoneroIncomingTransfer()
    {
        // nothing to initialize
    }

    public MoneroIncomingTransfer(MoneroIncomingTransfer transfer) : base(transfer)
    {
        _subaddressIndex = transfer._subaddressIndex;
        _address = transfer._address;
        _numSuggestedConfirmations = transfer._numSuggestedConfirmations;
    }

    public override MoneroIncomingTransfer Clone()
    {
        return new MoneroIncomingTransfer(this);
    }

    public override MoneroIncomingTransfer SetTx(MoneroTxWallet? tx)
    {
        _tx = tx;
        return this;
    }

    public override bool? IsIncoming()
    {
        return true;
    }

    public uint? GetSubaddressIndex()
    {
        return _subaddressIndex;
    }

    public MoneroIncomingTransfer SetSubaddressIndex(uint? subaddressIndex)
    {
        _subaddressIndex = subaddressIndex;
        return this;
    }

    public string? GetAddress()
    {
        return _address;
    }

    public MoneroIncomingTransfer SetAddress(string? address)
    {
        _address = address;
        return this;
    }

    public ulong? GetNumSuggestedConfirmations()
    {
        return _numSuggestedConfirmations;
    }

    public MoneroIncomingTransfer SetNumSuggestedConfirmations(ulong? numSuggestedConfirmations)
    {
        _numSuggestedConfirmations = numSuggestedConfirmations;
        return this;
    }

    public bool Equals(MoneroIncomingTransfer? other)
    {
        if (other == null)
        {
            return false;
        }

        if (other == this)
        {
            return true;
        }

        if (!base.Equals(other))
        {
            return false;
        }

        return _subaddressIndex == other._subaddressIndex &&
               _address == other._address &&
               _numSuggestedConfirmations == other._numSuggestedConfirmations;
    }

    public override string ToString(int indent)
    {
        var sb = new StringBuilder();
        sb.Append(base.ToString(indent) + "\n");
        sb.Append(GenUtils.KvLine("Subaddress index", GetSubaddressIndex(), indent));
        sb.Append(GenUtils.KvLine("Address", GetAddress(), indent));
        sb.Append(GenUtils.KvLine("Num suggested confirmations", GetNumSuggestedConfirmations(), indent));
        string str = sb.ToString();
        return str.Substring(0, str.Length - 1);
    }
}