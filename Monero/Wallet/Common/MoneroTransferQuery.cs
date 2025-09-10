using Monero.Common;

namespace Monero.Wallet.Common;

public class MoneroTransferQuery : MoneroTransfer
{
    private string? address;
    private List<string> addresses = [];
    private List<MoneroDestination> destinations = [];
    private bool? hasDestinations;
    private bool? isIncoming;
    private uint? subaddressIndex;
    private List<uint> subaddressIndices = [];
    protected MoneroTxQuery? txQuery;

    public MoneroTransferQuery()
    {
    }

    public MoneroTransferQuery(MoneroTransferQuery query) : base(query)
    {
        isIncoming = query.isIncoming;
        address = query.address;
        if (query.addresses != null)
        {
            addresses = new List<string>(query.addresses);
        }

        subaddressIndex = query.subaddressIndex;
        if (query.subaddressIndices != null)
        {
            subaddressIndices = new List<uint>(query.subaddressIndices);
        }

        if (query.destinations != null)
        {
            destinations = [];
            foreach (MoneroDestination destination in query.GetDestinations())
            {
                destinations.Add(destination.Clone());
            }
        }

        hasDestinations = query.hasDestinations;
        txQuery =
            query.txQuery; // reference original by default, MoneroTxQuery's deep copy will Set this to itself
        Validate();
    }

    private void Validate()
    {
        if (subaddressIndex != null && subaddressIndex < 0)
        {
            throw new MoneroError("Subaddress index must be >= 0");
        }

        if (subaddressIndices != null)
        {
            foreach (uint subaddressIdx in subaddressIndices)
            {
                if (subaddressIdx < 0)
                {
                    throw new MoneroError("Subaddress indices must be >= 0");
                }
            }
        }
    }

    public override MoneroTransferQuery Clone()
    {
        return new MoneroTransferQuery(this);
    }

    public MoneroTxQuery? GetTxQuery()
    {
        return txQuery;
    }

    public MoneroTransferQuery SetTxQuery(MoneroTxQuery txQuery)
    {
        this.txQuery = txQuery;
        if (txQuery != null)
        {
            txQuery.SetTransferQuery(this);
        }

        return this;
    }

    public override bool? IsIncoming()
    {
        return isIncoming;
    }

    public MoneroTransferQuery SetIsIncoming(bool isIncoming)
    {
        this.isIncoming = isIncoming;
        return this;
    }

    public override bool? IsOutgoing()
    {
        return isIncoming == null ? null : !isIncoming;
    }

    public MoneroTransferQuery SetIsOutgoing(bool isOutgoing)
    {
        isIncoming = isOutgoing == null ? null : !isOutgoing;
        return this;
    }

    public string? GetAddress()
    {
        return address;
    }

    public MoneroTransferQuery SetAddress(string address)
    {
        this.address = address;
        return this;
    }

    public List<string> GetAddresses()
    {
        return addresses;
    }

    public MoneroTransferQuery SetAddresses(List<string> addresses)
    {
        this.addresses = addresses;
        return this;
    }

    public override MoneroTransferQuery SetAccountIndex(uint? subaddressIdx)
    {
        base.SetAccountIndex(subaddressIdx);
        return this;
    }

    public uint? GetSubaddressIndex()
    {
        return subaddressIndex;
    }

    public MoneroTransferQuery SetSubaddressIndex(uint? subaddressIndex)
    {
        this.subaddressIndex = subaddressIndex;
        Validate();
        return this;
    }

    public List<uint> GetSubaddressIndices()
    {
        return subaddressIndices;
    }

    public MoneroTransferQuery SetSubaddressIndices(List<uint> subaddressIndices)
    {
        this.subaddressIndices = subaddressIndices;
        Validate();
        return this;
    }

    public List<MoneroDestination> GetDestinations()
    {
        return destinations;
    }

    public MoneroTransferQuery SetDestinations(List<MoneroDestination> destinations)
    {
        this.destinations = destinations;
        return this;
    }

    public bool? HasDestinations()
    {
        return hasDestinations;
    }

    public MoneroTransferQuery SetHasDestinations(bool hasDestinations)
    {
        this.hasDestinations = hasDestinations;
        return this;
    }

    public bool MeetsCriteria(MoneroTransfer transfer)
    {
        throw new NotImplementedException();
    }
}