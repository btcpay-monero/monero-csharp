namespace Monero.Wallet.Common;

public class MoneroOutputQuery : MoneroOutputWallet
{
    private ulong? _maxAmount;
    private ulong? _minAmount;
    private List<uint>? _subaddressIndices;
    protected MoneroTxQuery? _txQuery;

    public MoneroOutputQuery() { }

    public MoneroOutputQuery(MoneroOutputQuery query) : base(query)
    {
        if (query.GetMinAmount() != null)
        {
            _minAmount = query.GetMinAmount();
        }

        if (query.GetMaxAmount() != null)
        {
            _maxAmount = query.GetMaxAmount();
        }

        if (query._subaddressIndices != null)
        {
            _subaddressIndices = query._subaddressIndices;
        }

        _txQuery =
            query._txQuery; // to reference original by default, MoneroTxQuery's deep copy will Set this to itself
    }

    public override MoneroOutputQuery Clone()
    {
        return new MoneroOutputQuery(this);
    }

    public ulong? GetMinAmount()
    {
        return _minAmount;
    }

    public MoneroOutputQuery SetMinAmount(ulong minAmount)
    {
        _minAmount = minAmount;
        return this;
    }

    public ulong? GetMaxAmount()
    {
        return _maxAmount;
    }

    public MoneroOutputQuery SetMaxAmount(ulong maxAmount)
    {
        _maxAmount = maxAmount;
        return this;
    }

    public MoneroTxQuery? GetTxQuery()
    {
        return _txQuery;
    }

    public MoneroOutputQuery SetTxQuery(MoneroTxQuery? txQuery)
    {
        return SetTxQuery(txQuery, true);
    }

    public MoneroOutputQuery SetTxQuery(MoneroTxQuery? txQuery, bool setOutputQuery)
    {
        _txQuery = txQuery;
        if (setOutputQuery && txQuery != null)
        {
            txQuery.SetOutputQuery(this);
        }

        return this;
    }

    public List<uint>? GetSubaddressIndices()
    {
        return _subaddressIndices;
    }

    public MoneroOutputQuery SetSubaddressIndices(List<uint>? subaddressIndices)
    {
        _subaddressIndices = subaddressIndices;
        return this;
    }

    public bool MeetsCriteria(MoneroOutputWallet? output)
    {
        if (output == null)
        {
            return false;
        }

        // filter on output
        if (GetAccountIndex() != null && !GetAccountIndex().Equals(output.GetAccountIndex()))
        {
            return false;
        }

        if (GetSubaddressIndex() != null && !GetSubaddressIndex().Equals(output.GetSubaddressIndex()))
        {
            return false;
        }

        if (GetAmount() != null && ((uint)GetAmount()!).CompareTo(output.GetAmount()) != 0)
        {
            return false;
        }

        if (IsSpent() != null && !IsSpent().Equals(output.IsSpent()))
        {
            return false;
        }

        if (IsFrozen() != null && !IsFrozen().Equals(output.IsFrozen()))
        {
            return false;
        }

        // filter on output key image
        if (GetKeyImage() != null)
        {
            if (output.GetKeyImage() == null)
            {
                return false;
            }

            if (GetKeyImage()!.GetHex() != null && !GetKeyImage()!.GetHex()!.Equals(output.GetKeyImage()!.GetHex()))
            {
                return false;
            }

            if (GetKeyImage()!.GetSignature() != null &&
                !GetKeyImage()!.GetSignature()!.Equals(output.GetKeyImage()!.GetSignature()))
            {
                return false;
            }
        }

        // filter on extensions
        if (GetSubaddressIndices() != null && !GetSubaddressIndices()!.Contains((uint)output.GetSubaddressIndex()!))
        {
            return false;
        }

        // filter with tx query
        if (GetTxQuery() != null && !GetTxQuery()!.MeetsCriteria(output.GetTx()!, false))
        {
            return false;
        }

        // filter on remaining fields
        if (GetMinAmount() != null &&
            (output.GetAmount() == null || ((ulong)output.GetAmount()!).CompareTo(GetMinAmount()) < 0))
        {
            return false;
        }

        if (GetMaxAmount() != null &&
            (output.GetAmount() == null || ((ulong)output.GetAmount()!).CompareTo(GetMaxAmount()) > 0))
        {
            return false;
        }

        // output Meets query
        return true;
    }
}