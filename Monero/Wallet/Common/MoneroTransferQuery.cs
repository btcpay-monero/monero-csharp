using Monero.Common;

namespace Monero.Wallet.Common;

public class MoneroTransferQuery : MoneroTx
{
    private bool? _isLocked;
    private List<string>? _hashes;
    private ulong? _unlockTime;
    private ulong? _height;
    private bool? _includeOutputs;
    private ulong? _maxHeight;
    private ulong? _minHeight;
    private uint? _accountIndex;
    private string? _address;
    private List<string>? _addresses;
    private List<MoneroDestination>? _destinations;
    private bool? _hasDestinations;
    private bool? _isIncoming;
    private uint? _subaddressIndex;
    private List<uint>? _subaddressIndices;
    private string? _hash;

    public MoneroTransferQuery()
    {
    }

    public MoneroTransferQuery(MoneroTransferQuery query)
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
    }

    public override MoneroTransferQuery Clone()
    {
        return new MoneroTransferQuery(this);
    }

    public bool? IsIncoming()
    {
        return _isIncoming;
    }

    public MoneroTransferQuery SetIsIncoming(bool? isIncoming)
    {
        _isIncoming = isIncoming;
        return this;
    }

    public MoneroTransferQuery SetIsLocked(bool? locked)
    {
        _isLocked = locked;
        return this;
    }

    public bool? IsLocked()
    {
        return _isLocked;
    }

    public bool? IsOutgoing()
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

    public ulong? GetAccountIndex()
    {
        return _accountIndex;
    }

    public MoneroTransferQuery SetAccountIndex(uint? subaddressIdx)
    {
        _accountIndex = subaddressIdx;
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
        throw new NotImplementedException();
    }

    public override MoneroTransferQuery SetHash(string? hash)
    {
        _hash = hash;

        return this;
    }

    public override string? GetHash()
    {
        return _hash;
    }

    public override ulong? GetHeight()
    {
        return _height;
    }

    public MoneroTransferQuery SetHeight(ulong? height)
    {
        _height = height;
        return this;
    }

    public ulong? GetMinHeight()
    {
        return _minHeight;
    }

    public MoneroTransferQuery SetMinHeight(ulong? minHeight)
    {
        _minHeight = minHeight;
        return this;
    }

    public ulong? GetMaxHeight()
    {
        return _maxHeight;
    }

    public MoneroTransferQuery SetMaxHeight(ulong? maxHeight)
    {
        _maxHeight = maxHeight;
        return this;
    }

    public override MoneroTransferQuery SetUnlockTime(ulong? unlockTime)
    {
        _unlockTime = unlockTime;
        return this;
    }

    public bool? GetIncludeOutputs()
    {
        return _includeOutputs;
    }

    public MoneroTransferQuery SetIncludeOutputs(bool? includeOutputs)
    {
        _includeOutputs = includeOutputs;
        return this;
    }

    public List<string>? GetHashes()
    {
        return _hashes;
    }

    public MoneroTransferQuery SetHashes(List<string>? hashes)
    {
        _hashes = hashes;
        return this;
    }
}