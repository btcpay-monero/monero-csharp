namespace Monero.Wallet.Common;

public class MoneroTransferQuery : MoneroTransfer
{
    private string? _address;
    private List<string>? _addresses;
    private List<MoneroDestination>? _destinations;
    private bool? _hasDestinations;
    private bool? _isIncoming;
    private uint? _subaddressIndex;
    private List<uint>? _subaddressIndices;
    protected MoneroTxQuery? _txQuery;

    public MoneroTransferQuery()
    {
    }

    public MoneroTransferQuery(MoneroTransferQuery query) : base(query)
    {
        _isIncoming = query._isIncoming;
        _address = query._address;
        if (query._addresses != null)
        {
            _addresses = query._addresses;
        }

        _subaddressIndex = query._subaddressIndex;
        if (query._subaddressIndices != null)
        {
            _subaddressIndices = query._subaddressIndices;
        }

        if (query._destinations != null)
        {
            _destinations = [];
            foreach (MoneroDestination destination in query._destinations)
            {
                _destinations.Add(destination.Clone());
            }
        }

        _hasDestinations = query._hasDestinations;
        _txQuery =
            query._txQuery; // to reference original by default, MoneroTxQuery's deep copy will Set this to itself
    }

    public override MoneroTransferQuery Clone()
    {
        return new MoneroTransferQuery(this);
    }

    public MoneroTxQuery? GetTxQuery()
    {
        return _txQuery;
    }

    public MoneroTransferQuery SetTxQuery(MoneroTxQuery? txQuery)
    {
        this._txQuery = txQuery;
        if (txQuery != null)
        {
            txQuery.SetTransferQuery(this);
        }

        return this;
    }

    public override bool? IsIncoming()
    {
        return _isIncoming;
    }

    public MoneroTransferQuery SetIsIncoming(bool? isIncoming)
    {
        this._isIncoming = isIncoming;
        return this;
    }

    public override bool? IsOutgoing()
    {
        return _isIncoming == null ? null : !_isIncoming;
    }

    public MoneroTransferQuery SetIsOutgoing(bool? isOutgoing)
    {
        _isIncoming = isOutgoing == null ? null : !isOutgoing;
        return this;
    }

    public string? GetAddress()
    {
        return _address;
    }

    public MoneroTransferQuery SetAddress(string? address)
    {
        this._address = address;
        return this;
    }

    public List<string>? GetAddresses()
    {
        return _addresses;
    }

    public MoneroTransferQuery SetAddresses(List<string>? addresses)
    {
        this._addresses = addresses;
        return this;
    }

    public override MoneroTransferQuery SetAccountIndex(uint? subaddressIdx)
    {
        base.SetAccountIndex(subaddressIdx);
        return this;
    }

    public uint? GetSubaddressIndex()
    {
        return _subaddressIndex;
    }

    public MoneroTransferQuery SetSubaddressIndex(uint? subaddressIndex)
    {
        this._subaddressIndex = subaddressIndex;
        return this;
    }

    public List<uint>? GetSubaddressIndices()
    {
        return _subaddressIndices;
    }

    public MoneroTransferQuery SetSubaddressIndices(List<uint>? subaddressIndices)
    {
        this._subaddressIndices = subaddressIndices;
        return this;
    }

    public List<MoneroDestination>? GetDestinations()
    {
        return _destinations;
    }

    public MoneroTransferQuery SetDestinations(List<MoneroDestination>? destinations)
    {
        this._destinations = destinations;
        return this;
    }

    public bool? HasDestinations()
    {
        return _hasDestinations;
    }

    public MoneroTransferQuery SetHasDestinations(bool? hasDestinations)
    {
        this._hasDestinations = hasDestinations;
        return this;
    }

    public bool MeetsCriteria(MoneroTransfer? transfer)
    {
        throw new NotImplementedException();
    }
}