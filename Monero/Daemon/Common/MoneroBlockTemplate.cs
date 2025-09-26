using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroBlockTemplate
{
    [JsonPropertyName("blockhashing_blob")]
    [JsonInclude]
    private string? _blockHashingBlob { get; set; }
    [JsonPropertyName("blocktemplate_blob")]
    [JsonInclude]
    private string? _blockTemplateBlob { get; set; }
    [JsonPropertyName("difficulty")]
    [JsonInclude]
    private ulong? _difficulty { get; set; }
    [JsonPropertyName("expected_reward")]
    [JsonInclude]
    private ulong? _expectedReward { get; set; }
    [JsonPropertyName("height")]
    [JsonInclude]
    private ulong? _height { get; set; }
    [JsonPropertyName("next_seed_hash")]
    [JsonInclude]
    private string? _nextSeedHash { get; set; }
    [JsonPropertyName("prev_hash")]
    [JsonInclude]
    private string? _prevHash { get; set; }
    [JsonPropertyName("reserved_offset")]
    [JsonInclude]
    private ulong? _reservedOffset { get; set; }
    [JsonPropertyName("seed_hash")]
    [JsonInclude]
    private string? _seedHash { get; set; }
    [JsonPropertyName("seed_height")]
    [JsonInclude]
    private ulong? _seedHeight { get; set; }

    public string? GetBlockTemplateBlob()
    {
        return _blockTemplateBlob;
    }

    public void SetBlockTemplateBlob(string? blockTemplateBlob)
    {
        _blockTemplateBlob = blockTemplateBlob;
    }

    public string? GetBlockHashingBlob()
    {
        return _blockHashingBlob;
    }

    public void SetBlockHashingBlob(string? blockHashingBlob)
    {
        _blockHashingBlob = blockHashingBlob;
    }

    public ulong? GetDifficulty()
    {
        return _difficulty;
    }

    public void SetDifficulty(ulong? difficulty)
    {
        _difficulty = difficulty;
    }

    public ulong? GetExpectedReward()
    {
        return _expectedReward;
    }

    public void SetExpectedReward(ulong? expectedReward)
    {
        _expectedReward = expectedReward;
    }

    public ulong? GetHeight()
    {
        return _height;
    }

    public void SetHeight(ulong? height)
    {
        _height = height;
    }

    public string? GetPrevHash()
    {
        return _prevHash;
    }

    public void SetPrevHash(string? prevHash)
    {
        _prevHash = prevHash;
    }

    public ulong? GetReservedOffset()
    {
        return _reservedOffset;
    }

    public void SetReservedOffset(ulong? reservedOffset)
    {
        _reservedOffset = reservedOffset;
    }

    public ulong? GetSeedHeight()
    {
        return _seedHeight;
    }

    public void SetSeedHeight(ulong? seedHeight)
    {
        _seedHeight = seedHeight;
    }

    public string? GetSeedHash()
    {
        return _seedHash;
    }

    public void SetSeedHash(string? seedHash)
    {
        _seedHash = seedHash;
    }

    public string? GetNextSeedHash()
    {
        return _nextSeedHash;
    }

    public void SetNextSeedHash(string? nextSeedHash)
    {
        _nextSeedHash = nextSeedHash;
    }
}