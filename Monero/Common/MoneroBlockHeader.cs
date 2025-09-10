
namespace Monero.Common;

public class MoneroBlockHeader
{
    private string? hash;
    private ulong? height;
    private ulong? timestamp;
    private ulong? size;
    private ulong? weight;
    private ulong? longTermWeight;
    private ulong? depth;
    private ulong? difficulty;
    private ulong? cumulativeDifficulty;
    private uint? majorVersion;
    private uint? minorVersion;
    private ulong? nonce;
    private string? minerTxHash;
    private uint? numTxs;
    private bool? orphanStatus;
    private string? prevHash;
    private ulong? reward;
    private string? powHash;

    public MoneroBlockHeader() { }

    public MoneroBlockHeader(MoneroBlockHeader header)
    {
        hash = header.hash;
        height = header.height;
        timestamp = header.timestamp;
        size = header.size;
        weight = header.weight;
        longTermWeight = header.longTermWeight;
        depth = header.depth;
        difficulty = header.difficulty;
        cumulativeDifficulty = header.cumulativeDifficulty;
        majorVersion = header.majorVersion;
        minorVersion = header.minorVersion;
        nonce = header.nonce;
        numTxs = header.numTxs;
        orphanStatus = header.orphanStatus;
        prevHash = header.prevHash;
        reward = header.reward;
        powHash = header.powHash;
    }

    public string? GetHash()
    {
        return hash;
    }

    public virtual MoneroBlockHeader SetHash(string? hash)
    {
        this.hash = hash;
        return this;
    }

    public ulong? GetHeight()
    {
        return height;
    }

    public virtual MoneroBlockHeader SetHeight(ulong? height)
    {
        this.height = height;
        return this;
    }

    public ulong? GetTimestamp()
    {
        return timestamp;
    }

    public virtual MoneroBlockHeader SetTimestamp(ulong? timestamp)
    {
        this.timestamp = timestamp;
        return this;
    }

    public ulong? GetSize()
    {
        return size;
    }

    public virtual MoneroBlockHeader SetSize(ulong? size)
    {
        this.size = size;
        return this;
    }

    public ulong? GetWeight()
    {
        return weight;
    }

    public virtual MoneroBlockHeader SetWeight(ulong? weight)
    {
        this.weight = weight;
        return this;
    }

    public ulong? GetLongTermWeight()
    {
        return longTermWeight;
    }

    public virtual MoneroBlockHeader SetLongTermWeight(ulong? longTermWeight)
    {
        this.longTermWeight = longTermWeight;
        return this;
    }

    public ulong? GetDepth()
    {
        return depth;
    }

    public virtual MoneroBlockHeader SetDepth(ulong? depth)
    {
        this.depth = depth;
        return this;
    }

    public ulong? GetDifficulty()
    {
        return difficulty;
    }

    public virtual MoneroBlockHeader SetDifficulty(ulong? difficulty)
    {
        this.difficulty = difficulty;
        return this;
    }

    public ulong? GetCumulativeDifficulty()
    {
        return cumulativeDifficulty;
    }

    public virtual MoneroBlockHeader SetCumulativeDifficulty(ulong? cumulativeDifficulty)
    {
        this.cumulativeDifficulty = cumulativeDifficulty;
        return this;
    }

    public uint? GetMajorVersion()
    {
        return majorVersion;
    }

    public virtual MoneroBlockHeader SetMajorVersion(uint? majorVersion)
    {
        this.majorVersion = majorVersion;
        return this;
    }

    public uint? GetMinorVersion()
    {
        return minorVersion;
    }

    public virtual MoneroBlockHeader SetMinorVersion(uint? minorVersion)
    {
        this.minorVersion = minorVersion;
        return this;
    }

    public ulong? GetNonce()
    {
        return nonce;
    }

    public virtual MoneroBlockHeader SetNonce(ulong? nonce)
    {
        this.nonce = nonce;
        return this;
    }

    public string? GetMinerTxHash()
    {
        return minerTxHash;
    }

    public virtual MoneroBlockHeader SetMinerTxHash(string? minerTxHash)
    {
        this.minerTxHash = minerTxHash;
        return this;
    }

    public uint? GetNumTxs()
    {
        return numTxs;
    }

    public virtual MoneroBlockHeader SetNumTxs(uint? numTxs)
    {
        this.numTxs = numTxs;
        return this;
    }

    public bool? GetOrphanStatus()
    {
        return orphanStatus;
    }

    public virtual MoneroBlockHeader SetOrphanStatus(bool? orphanStatus)
    {
        this.orphanStatus = orphanStatus;
        return this;
    }

    public string? GetPrevHash()
    {
        return prevHash;
    }

    public virtual MoneroBlockHeader SetPrevHash(string? prevHash)
    {
        this.prevHash = prevHash;
        return this;
    }

    public ulong? GetReward()
    {
        return reward;
    }

    public virtual MoneroBlockHeader SetReward(ulong? reward)
    {
        this.reward = reward;
        return this;
    }

    public string? GetPowHash()
    {
        return powHash;
    }

    public virtual MoneroBlockHeader SetPowHash(string? powHash)
    {
        this.powHash = powHash;
        return this;
    }

    public virtual MoneroBlockHeader Merge(MoneroBlockHeader? header)
    {
        if (header == null) throw new ArgumentNullException(nameof(header), "Cannot merge null header into block header");
        if (this == header) return this;
        this.SetHash(GenUtils.Reconcile(this.GetHash(), header.GetHash()));
        this.SetHeight(GenUtils.Reconcile(this.GetHeight(), header.GetHeight(), null, null, true));  // height can increase
        this.SetTimestamp(GenUtils.Reconcile(this.GetTimestamp(), header.GetTimestamp(), null, null, true));  // block timestamp can increase
        this.SetSize(GenUtils.Reconcile(this.GetSize(), header.GetSize()));
        this.SetWeight(GenUtils.Reconcile(this.GetWeight(), header.GetWeight()));
        this.SetDepth(GenUtils.Reconcile(this.GetDepth(), header.GetDepth()));
        this.SetDifficulty(GenUtils.Reconcile(this.GetDifficulty(), header.GetDifficulty()));
        this.SetCumulativeDifficulty(GenUtils.Reconcile(this.GetCumulativeDifficulty(), header.GetCumulativeDifficulty()));
        this.SetMajorVersion(GenUtils.Reconcile(this.GetMajorVersion(), header.GetMajorVersion()));
        this.SetMinorVersion(GenUtils.Reconcile(this.GetMinorVersion(), header.GetMinorVersion()));
        this.SetNonce(GenUtils.Reconcile(this.GetNonce(), header.GetNonce()));
        this.SetMinerTxHash(GenUtils.Reconcile(this.GetMinerTxHash(), header.GetMinerTxHash()));
        this.SetNumTxs(GenUtils.Reconcile(this.GetNumTxs(), header.GetNumTxs()));
        this.SetOrphanStatus(GenUtils.Reconcile(this.GetOrphanStatus(), header.GetOrphanStatus()));
        this.SetPrevHash(GenUtils.Reconcile(this.GetPrevHash(), header.GetPrevHash()));
        this.SetReward(GenUtils.Reconcile(this.GetReward(), header.GetReward()));
        this.SetPowHash(GenUtils.Reconcile(this.GetPowHash(), header.GetPowHash()));
        return this;
    }

    public virtual bool Equals(MoneroBlockHeader? other)
    {
        if (other == null) return false;
        if (other == this) return true;
        return hash == other.hash &&
               height == other.height &&
               timestamp == other.timestamp &&
               size == other.size &&
               weight == other.weight &&
               longTermWeight == other.longTermWeight &&
               depth == other.depth &&
               difficulty == other.difficulty &&
               cumulativeDifficulty == other.cumulativeDifficulty &&
               majorVersion == other.majorVersion &&
               minorVersion == other.minorVersion &&
               nonce == other.nonce &&
               minerTxHash == other.minerTxHash &&
               numTxs == other.numTxs &&
               orphanStatus == other.orphanStatus &&
               prevHash == other.prevHash &&
               reward == other.reward &&
               powHash == other.powHash;
    }
}