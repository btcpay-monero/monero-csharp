namespace Monero.Wallet.Common;

public class MoneroOutputQuery : MoneroOutputWallet
{
    private ulong? maxAmount;
    private ulong? minAmount;
    private List<uint> subaddressIndices = [];
    protected MoneroTxQuery? txQuery;

    public MoneroOutputQuery() { }

    public MoneroOutputQuery(MoneroOutputQuery query) : base(query)
    {
        if (query.GetMinAmount() != null)
        {
            minAmount = query.GetMinAmount();
        }

        if (query.GetMaxAmount() != null)
        {
            maxAmount = query.GetMaxAmount();
        }

        if (query.subaddressIndices != null)
        {
            subaddressIndices = new List<uint>(query.subaddressIndices);
        }

        txQuery =
            query.txQuery; // reference original by default, MoneroTxQuery's deep copy will Set this to itself
    }

    public override MoneroOutputQuery Clone()
    {
        return new MoneroOutputQuery(this);
    }

    public ulong? GetMinAmount()
    {
        return minAmount;
    }

    public MoneroOutputQuery SetMinAmount(ulong minAmount)
    {
        this.minAmount = minAmount;
        return this;
    }

    public ulong? GetMaxAmount()
    {
        return maxAmount;
    }

    public MoneroOutputQuery SetMaxAmount(ulong maxAmount)
    {
        this.maxAmount = maxAmount;
        return this;
    }

    public MoneroTxQuery? GetTxQuery()
    {
        return txQuery;
    }

    public MoneroOutputQuery SetTxQuery(MoneroTxQuery? txQuery)
    {
        this.txQuery = txQuery;
        if (txQuery != null)
        {
            txQuery.SetOutputQuery(this);
        }

        return this;
    }

    public List<uint> GetSubaddressIndices()
    {
        return subaddressIndices;
    }

    public MoneroOutputQuery? SetSubaddressIndices(List<uint> subaddressIndices)
    {
        this.subaddressIndices = subaddressIndices;
        return this;
    }

    public bool MeetsCriteria(MoneroOutputWallet output)
    {
        throw new NotImplementedException();
    }
}