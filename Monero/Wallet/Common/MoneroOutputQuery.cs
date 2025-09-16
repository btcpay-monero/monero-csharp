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
            _subaddressIndices = new List<uint>(query._subaddressIndices);
        }

        _txQuery =
            query._txQuery; // reference original by default, MoneroTxQuery's deep copy will Set this to itself
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
        this._minAmount = minAmount;
        return this;
    }

    public ulong? GetMaxAmount()
    {
        return _maxAmount;
    }

    public MoneroOutputQuery SetMaxAmount(ulong maxAmount)
    {
        this._maxAmount = maxAmount;
        return this;
    }

    public MoneroTxQuery? GetTxQuery()
    {
        return _txQuery;
    }

    public MoneroOutputQuery SetTxQuery(MoneroTxQuery? txQuery)
    {
        this._txQuery = txQuery;
        if (txQuery != null)
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
        this._subaddressIndices = subaddressIndices;
        return this;
    }

    public bool MeetsCriteria(MoneroOutputWallet? output)
    {
        throw new NotImplementedException();
    }
}