using System.Text.Json.Serialization;

namespace Monero.Common;

public class MoneroBlockHeader : IEquatable<MoneroBlockHeader>
{
    [JsonPropertyName("cumulative_difficulty")]
    public ulong? CumulativeDifficulty { get; set; }

    [JsonPropertyName("depth")]
    public ulong? Depth { get; set; }

    [JsonPropertyName("difficulty")]
    public ulong? Difficulty { get; set; }

    [JsonPropertyName("hash")]
    public string? Hash { get; set; }

    [JsonPropertyName("height")]
    public ulong? Height { get; set; }

    [JsonPropertyName("long_term_weight")]
    public ulong? LongTermWeight { get; set; }

    [JsonPropertyName("major_version")]
    public uint? MajorVersion { get; set; }

    [JsonPropertyName("miner_tx_hash")]
    public string? MinerTxHash { get; set; }

    [JsonPropertyName("minor_version")]
    public uint? MinorVersion { get; set; }

    [JsonPropertyName("nonce")]
    public ulong? Nonce { get; set; }

    [JsonPropertyName("num_txes")]
    public uint? NumTxs { get; set; }

    [JsonPropertyName("orphan_status")]
    public bool? OrphanStatus { get; set; }

    [JsonPropertyName("pow_hash")]
    public string? PowHash { get; set; }

    [JsonPropertyName("prev_hash")]
    public string? PrevHash { get; set; }

    [JsonPropertyName("reward")]
    public ulong? Reward { get; set; }

    [JsonPropertyName("block_size")]
    public ulong? Size { get; set; }

    [JsonPropertyName("timestamp")]
    public ulong? Timestamp { get; set; }

    [JsonPropertyName("block_weight")]
    public ulong? Weight { get; set; }

    public MoneroBlockHeader() { }

    public MoneroBlockHeader(MoneroBlockHeader header)
    {
        Hash = header.Hash;
        Height = header.Height;
        Timestamp = header.Timestamp;
        Size = header.Size;
        Weight = header.Weight;
        LongTermWeight = header.LongTermWeight;
        Depth = header.Depth;
        Difficulty = header.Difficulty;
        CumulativeDifficulty = header.CumulativeDifficulty;
        MajorVersion = header.MajorVersion;
        MinorVersion = header.MinorVersion;
        Nonce = header.Nonce;
        NumTxs = header.NumTxs;
        OrphanStatus = header.OrphanStatus;
        PrevHash = header.PrevHash;
        Reward = header.Reward;
        PowHash = header.PowHash;
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

        return Hash == other.Hash &&
               Height == other.Height &&
               Timestamp == other.Timestamp &&
               Size == other.Size &&
               Weight == other.Weight &&
               LongTermWeight == other.LongTermWeight &&
               Depth == other.Depth &&
               Difficulty == other.Difficulty &&
               CumulativeDifficulty == other.CumulativeDifficulty &&
               MajorVersion == other.MajorVersion &&
               MinorVersion == other.MinorVersion &&
               Nonce == other.Nonce &&
               MinerTxHash == other.MinerTxHash &&
               NumTxs == other.NumTxs &&
               OrphanStatus == other.OrphanStatus &&
               PrevHash == other.PrevHash &&
               Reward == other.Reward &&
               PowHash == other.PowHash;
    }

    public string? GetHash()
    {
        return Hash;
    }

    public virtual MoneroBlockHeader SetHash(string? hash)
    {
        Hash = hash;
        return this;
    }

    public ulong? GetHeight()
    {
        return Height;
    }

    public virtual MoneroBlockHeader SetHeight(ulong? height)
    {
        Height = height;
        return this;
    }

    public ulong? GetTimestamp()
    {
        return Timestamp;
    }

    public virtual MoneroBlockHeader SetTimestamp(ulong? timestamp)
    {
        Timestamp = timestamp;
        return this;
    }

    public ulong? GetSize()
    {
        return Size;
    }

    public virtual MoneroBlockHeader SetSize(ulong? size)
    {
        Size = size;
        return this;
    }

    public ulong? GetWeight()
    {
        return Weight;
    }

    public virtual MoneroBlockHeader SetWeight(ulong? weight)
    {
        Weight = weight;
        return this;
    }

    public ulong? GetLongTermWeight()
    {
        return LongTermWeight;
    }

    public virtual MoneroBlockHeader SetLongTermWeight(ulong? longTermWeight)
    {
        LongTermWeight = longTermWeight;
        return this;
    }

    public ulong? GetDepth()
    {
        return Depth;
    }

    public virtual MoneroBlockHeader SetDepth(ulong? depth)
    {
        Depth = depth;
        return this;
    }

    public ulong? GetDifficulty()
    {
        return Difficulty;
    }

    public virtual MoneroBlockHeader SetDifficulty(ulong? difficulty)
    {
        Difficulty = difficulty;
        return this;
    }

    public ulong? GetCumulativeDifficulty()
    {
        return CumulativeDifficulty;
    }

    public virtual MoneroBlockHeader SetCumulativeDifficulty(ulong? cumulativeDifficulty)
    {
        CumulativeDifficulty = cumulativeDifficulty;
        return this;
    }

    public uint? GetMajorVersion()
    {
        return MajorVersion;
    }

    public virtual MoneroBlockHeader SetMajorVersion(uint? majorVersion)
    {
        MajorVersion = majorVersion;
        return this;
    }

    public uint? GetMinorVersion()
    {
        return MinorVersion;
    }

    public virtual MoneroBlockHeader SetMinorVersion(uint? minorVersion)
    {
        MinorVersion = minorVersion;
        return this;
    }

    public ulong? GetNonce()
    {
        return Nonce;
    }

    public virtual MoneroBlockHeader SetNonce(ulong? nonce)
    {
        Nonce = nonce;
        return this;
    }

    public string? GetMinerTxHash()
    {
        return MinerTxHash;
    }

    public virtual MoneroBlockHeader SetMinerTxHash(string? minerTxHash)
    {
        MinerTxHash = minerTxHash;
        return this;
    }

    public uint? GetNumTxs()
    {
        return NumTxs;
    }

    public virtual MoneroBlockHeader SetNumTxs(uint? numTxs)
    {
        NumTxs = numTxs;
        return this;
    }

    public bool? GetOrphanStatus()
    {
        return OrphanStatus;
    }

    public virtual MoneroBlockHeader SetOrphanStatus(bool? orphanStatus)
    {
        OrphanStatus = orphanStatus;
        return this;
    }

    public string? GetPrevHash()
    {
        return PrevHash;
    }

    public virtual MoneroBlockHeader SetPrevHash(string? prevHash)
    {
        PrevHash = prevHash;
        return this;
    }

    public ulong? GetReward()
    {
        return Reward;
    }

    public virtual MoneroBlockHeader SetReward(ulong? reward)
    {
        Reward = reward;
        return this;
    }

    public string? GetPowHash()
    {
        return PowHash;
    }

    public virtual MoneroBlockHeader SetPowHash(string? powHash)
    {
        PowHash = powHash;
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

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}