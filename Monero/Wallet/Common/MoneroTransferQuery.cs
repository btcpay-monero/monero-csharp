using Monero.Common;

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
        if (txQuery == null)
        {
            return SetTxQuery(null, true);
        }

        return SetTxQuery(txQuery, true);
    }

    public MoneroTransferQuery SetTxQuery(MoneroTxQuery? query, bool setTransferQuery)
    {
        _txQuery = query;
        if (setTransferQuery && _txQuery != null)
        {
            _txQuery.SetTransferQuery(this);
        }
        return this;
    }

    public override bool? IsIncoming()
    {
        return _isIncoming;
    }

    public MoneroTransferQuery SetIsIncoming(bool? isIncoming)
    {
        _isIncoming = isIncoming;
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
        _address = address;
        return this;
    }

    public List<string>? GetAddresses()
    {
        return _addresses;
    }

    public MoneroTransferQuery SetAddresses(List<string>? addresses)
    {
        _addresses = addresses;
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
        _subaddressIndex = subaddressIndex;
        return this;
    }

    public List<uint>? GetSubaddressIndices()
    {
        return _subaddressIndices;
    }

    public MoneroTransferQuery SetSubaddressIndices(List<uint>? subaddressIndices)
    {
        _subaddressIndices = subaddressIndices;
        return this;
    }

    public List<MoneroDestination>? GetDestinations()
    {
        return _destinations;
    }

    public MoneroTransferQuery SetDestinations(List<MoneroDestination>? destinations)
    {
        _destinations = destinations;
        return this;
    }

    public bool? HasDestinations()
    {
        return _hasDestinations;
    }

    public MoneroTransferQuery SetHasDestinations(bool? hasDestinations)
    {
        _hasDestinations = hasDestinations;
        return this;
    }

    public bool MeetsCriteria(MoneroTransfer? transfer)
    {
        return MeetsCriteria(transfer, true);
    }

    public bool MeetsCriteria(MoneroTransfer? transfer, bool queryParent)
    {
        if (transfer == null)
        {
            throw new MoneroError("transfer is null");
        }

        // filter on common fields
        if (IsIncoming() != null && IsIncoming() != transfer.IsIncoming())
        {
            return false;
        }

        if (IsOutgoing() != null && IsOutgoing() != transfer.IsOutgoing())
        {
            return false;
        }

        if (GetAmount() != null && ((ulong)GetAmount()!).CompareTo(transfer.GetAmount()) != 0)
        {
            return false;
        }

        if (GetAccountIndex() != null && !GetAccountIndex().Equals(transfer.GetAccountIndex()))
        {
            return false;
        }

        // filter on incoming fields
        if (transfer is MoneroIncomingTransfer inTransfer)
        {
            if (HasDestinations() == true)
            {
                return false;
            }

            if (GetAddress() != null && !GetAddress()!.Equals(inTransfer.GetAddress()))
            {
                return false;
            }

            if (GetAddresses() != null && !GetAddresses()!.Contains(inTransfer.GetAddress()!))
            {
                return false;
            }

            if (GetSubaddressIndex() != null && !GetSubaddressIndex().Equals(inTransfer.GetSubaddressIndex()))
            {
                return false;
            }

            if (GetSubaddressIndices() != null &&
              !GetSubaddressIndices()!.Contains((uint)inTransfer.GetSubaddressIndex()!))
            {
                return false;
            }
        }

        // filter on outgoing fields
        else if (transfer is MoneroOutgoingTransfer outTransfer)
        {
            // filter on addresses
            if (GetAddress() != null && (outTransfer.GetAddresses() == null || !outTransfer.GetAddresses()!.Contains(GetAddress()!)))
            {
                return false;   // TODO: will filter all transfers if they don't contain addresses
            }
            if (GetAddresses() != null)
            {
                HashSet<string> intersections = new(GetAddresses()!);
                intersections.IntersectWith(outTransfer.GetAddresses()!);
                if (intersections.Count == 0)
                {
                    return false;  // must have overlapping addresses
                }
            }

            // filter on subaddress indices
            if (GetSubaddressIndex() != null && (outTransfer.GetSubaddressIndices() == null ||
                                               !outTransfer.GetSubaddressIndices()!.Contains(
                                                   (uint)GetSubaddressIndex()!)))
            {
                return false;
            }
            if (GetSubaddressIndices() != null)
            {
                HashSet<uint> intersections = new(GetSubaddressIndices()!);
                intersections.IntersectWith(outTransfer.GetSubaddressIndices()!);
                if (intersections.Count == 0)
                {
                    return false;  // must have overlapping subaddress indices
                }
            }

            // filter on having destinations
            if (HasDestinations() != null)
            {
                if (HasDestinations() == true && outTransfer.GetDestinations() == null)
                {
                    return false;
                }

                if (!HasDestinations() == true && outTransfer.GetDestinations() != null)
                {
                    return false;
                }
            }

        }
        // otherwise invalid type
        else
        {
            throw new Exception("Transfer must be MoneroIncomingTransfer or MoneroOutgoingTransfer");
        }

        // filter with tx filter
        if (queryParent && GetTxQuery() != null && !GetTxQuery()!.MeetsCriteria(transfer.GetTx()!, false))
        {
            return false;
        }
        return true;
    }
}