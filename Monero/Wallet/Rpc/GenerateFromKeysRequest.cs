using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class GenerateFromKeysRequest
{
    [JsonPropertyName("address")] public string? PrimaryAddress { get; set; }
    [JsonPropertyName("viewkey")] public string? PrivateViewKey { get; set; }
    [JsonPropertyName("filename")] public string? WalletFileName { get; set; }
    [JsonPropertyName("spendkey")] public string? SpendKey { get; set; }
    [JsonPropertyName("restore_height")] public uint? RestoreHeight { get; set; }
    [JsonPropertyName("password")] public string? Password { get; set; }
    [JsonPropertyName("autosave_current")] public bool? AutosaveCurrent { get; set; }
}