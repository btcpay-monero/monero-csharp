namespace Monero.Common;

public class MoneroBlockHeader : IEquatable<MoneroBlockHeader>
{
    private ulong? _cumulativeDifficulty;
    private ulong? _depth;
    private ulong? _difficulty;
    private string? _hash;
    private ulong? _height;
    private ulong? _longTermWeight;
    private uint? _majorVersion;
    private string? _minerTxHash;
    private uint? _minorVersion;
    private ulong? _nonce;
    private uint? _numTxs;
    private bool? _orphanStatus;
    private string? _powHash;
    private string? _prevHash;
    private ulong? _reward;
    private ulong? _size;
    private ulong? _timestamp;
    private ulong? _weight;

    public MoneroBlockHeader() { }

    public MoneroBlockHeader(MoneroBlockHeader header)
    {
        _hash = header._hash;
        _height = header._height;
        _timestamp = header._timestamp;
        _size = header._size;
        _weight = header._weight;
        _longTermWeight = header._longTermWeight;
        _depth = header._depth;
        _difficulty = header._difficulty;
        _cumulativeDifficulty = header._cumulativeDifficulty;
        _majorVersion = header._majorVersion;
        _minorVersion = header._minorVersion;
        _nonce = header._nonce;
        _numTxs = header._numTxs;
        _orphanStatus = header._orphanStatus;
        _prevHash = header._prevHash;
        _reward = header._reward;
        _powHash = header._powHash;
    }

    public string? GetHash()
    {
        return _hash;
    }

    public virtual MoneroBlockHeader SetHash(string? hash)
    {
        this._hash = hash;
        return this;
    }

    public ulong? GetHeight()
    {
        return _height;
    }

    public virtual MoneroBlockHeader SetHeight(ulong? height)
    {
        this._height = height;
        return this;
    }

    public ulong? GetTimestamp()
    {
        return _timestamp;
    }

    public virtual MoneroBlockHeader SetTimestamp(ulong? timestamp)
    {
        this._timestamp = timestamp;
        return this;
    }

    public ulong? GetSize()
    {
        return _size;
    }

    public virtual MoneroBlockHeader SetSize(ulong? size)
    {
        this._size = size;
        return this;
    }

    public ulong? GetWeight()
    {
        return _weight;
    }

    public virtual MoneroBlockHeader SetWeight(ulong? weight)
    {
        this._weight = weight;
        return this;
    }

    public ulong? GetLongTermWeight()
    {
        return _longTermWeight;
    }

    public virtual MoneroBlockHeader SetLongTermWeight(ulong? longTermWeight)
    {
        this._longTermWeight = longTermWeight;
        return this;
    }

    public ulong? GetDepth()
    {
        return _depth;
    }

    public virtual MoneroBlockHeader SetDepth(ulong? depth)
    {
        this._depth = depth;
        return this;
    }

    public ulong? GetDifficulty()
    {
        return _difficulty;
    }

    public virtual MoneroBlockHeader SetDifficulty(ulong? difficulty)
    {
        this._difficulty = difficulty;
        return this;
    }

    public ulong? GetCumulativeDifficulty()
    {
        return _cumulativeDifficulty;
    }

    public virtual MoneroBlockHeader SetCumulativeDifficulty(ulong? cumulativeDifficulty)
    {
        this._cumulativeDifficulty = cumulativeDifficulty;
        return this;
    }

    public uint? GetMajorVersion()
    {
        return _majorVersion;
    }

    public virtual MoneroBlockHeader SetMajorVersion(uint? majorVersion)
    {
        this._majorVersion = majorVersion;
        return this;
    }

    public uint? GetMinorVersion()
    {
        return _minorVersion;
    }

    public virtual MoneroBlockHeader SetMinorVersion(uint? minorVersion)
    {
        this._minorVersion = minorVersion;
        return this;
    }

    public ulong? GetNonce()
    {
        return _nonce;
    }

    public virtual MoneroBlockHeader SetNonce(ulong? nonce)
    {
        this._nonce = nonce;
        return this;
    }

    public string? GetMinerTxHash()
    {
        return _minerTxHash;
    }

    public virtual MoneroBlockHeader SetMinerTxHash(string? minerTxHash)
    {
        this._minerTxHash = minerTxHash;
        return this;
    }

    public uint? GetNumTxs()
    {
        return _numTxs;
    }

    public virtual MoneroBlockHeader SetNumTxs(uint? numTxs)
    {
        this._numTxs = numTxs;
        return this;
    }

    public bool? GetOrphanStatus()
    {
        return _orphanStatus;
    }

    public virtual MoneroBlockHeader SetOrphanStatus(bool? orphanStatus)
    {
        this._orphanStatus = orphanStatus;
        return this;
    }

    public string? GetPrevHash()
    {
        return _prevHash;
    }

    public virtual MoneroBlockHeader SetPrevHash(string? prevHash)
    {
        this._prevHash = prevHash;
        return this;
    }

    public ulong? GetReward()
    {
        return _reward;
    }

    public virtual MoneroBlockHeader SetReward(ulong? reward)
    {
        this._reward = reward;
        return this;
    }

    public string? GetPowHash()
    {
        return _powHash;
    }

    public virtual MoneroBlockHeader SetPowHash(string? powHash)
    {
        this._powHash = powHash;
        return this;
    }

    public virtual MoneroBlockHeader Merge(MoneroBlockHeader? header)
    {
        if (header == null)
        {
            throw new ArgumentNullException(nameof(header), "Cannot merge null header into block header");
        }

        if (this == header)
        {
            return this;
        }

        SetHash(GenUtils.Reconcile(GetHash(), header.GetHash()));
        SetHeight(GenUtils.Reconcile(GetHeight(), header.GetHeight(), null, null,
            true)); // height can increase
        SetTimestamp(GenUtils.Reconcile(GetTimestamp(), header.GetTimestamp(), null, null,
            true)); // block timestamp can increase
        SetSize(GenUtils.Reconcile(GetSize(), header.GetSize()));
        SetWeight(GenUtils.Reconcile(GetWeight(), header.GetWeight()));
        SetDepth(GenUtils.Reconcile(GetDepth(), header.GetDepth()));
        SetDifficulty(GenUtils.Reconcile(GetDifficulty(), header.GetDifficulty()));
        SetCumulativeDifficulty(GenUtils.Reconcile(GetCumulativeDifficulty(),
            header.GetCumulativeDifficulty()));
        SetMajorVersion(GenUtils.Reconcile(GetMajorVersion(), header.GetMajorVersion()));
        SetMinorVersion(GenUtils.Reconcile(GetMinorVersion(), header.GetMinorVersion()));
        SetNonce(GenUtils.Reconcile(GetNonce(), header.GetNonce()));
        SetMinerTxHash(GenUtils.Reconcile(GetMinerTxHash(), header.GetMinerTxHash()));
        SetNumTxs(GenUtils.Reconcile(GetNumTxs(), header.GetNumTxs()));
        SetOrphanStatus(GenUtils.Reconcile(GetOrphanStatus(), header.GetOrphanStatus()));
        SetPrevHash(GenUtils.Reconcile(GetPrevHash(), header.GetPrevHash()));
        SetReward(GenUtils.Reconcile(GetReward(), header.GetReward()));
        SetPowHash(GenUtils.Reconcile(GetPowHash(), header.GetPowHash()));
        return this;
    }

    public override bool Equals(object? other)
    {
        return Equals(other as MoneroBlockHeader);
    }

    public virtual bool Equals(MoneroBlockHeader? other)
    {
        if (other == null)
        {
            return false;
        }

        if (other == this)
        {
            return true;
        }

        return _hash == other._hash &&
               _height == other._height &&
               _timestamp == other._timestamp &&
               _size == other._size &&
               _weight == other._weight &&
               _longTermWeight == other._longTermWeight &&
               _depth == other._depth &&
               _difficulty == other._difficulty &&
               _cumulativeDifficulty == other._cumulativeDifficulty &&
               _majorVersion == other._majorVersion &&
               _minorVersion == other._minorVersion &&
               _nonce == other._nonce &&
               _minerTxHash == other._minerTxHash &&
               _numTxs == other._numTxs &&
               _orphanStatus == other._orphanStatus &&
               _prevHash == other._prevHash &&
               _reward == other._reward &&
               _powHash == other._powHash;
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}