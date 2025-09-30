using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroBlockTemplate
{
    [JsonPropertyName("blockhashing_blob")]
    public string? BlockHashingBlob { get; set; }

    [JsonPropertyName("blocktemplate_blob")]
    public string? BlockTemplateBlob { get; set; }

    [JsonPropertyName("difficulty")]
    public ulong? Difficulty { get; set; }

    [JsonPropertyName("expected_reward")]
    public ulong? ExpectedReward { get; set; }

    [JsonPropertyName("height")]
    public ulong? Height { get; set; }

    [JsonPropertyName("next_seed_hash")]
    public string? NextSeedHash { get; set; }

    [JsonPropertyName("prev_hash")]
    public string? PrevHash { get; set; }

    [JsonPropertyName("reserved_offset")]
    public ulong? ReservedOffset { get; set; }

    [JsonPropertyName("seed_hash")]
    public string? SeedHash { get; set; }

    [JsonPropertyName("seed_height")]
    public ulong? SeedHeight { get; set; }
}