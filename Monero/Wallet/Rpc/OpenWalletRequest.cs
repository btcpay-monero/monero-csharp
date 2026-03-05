using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class OpenWalletRequest
{
    [JsonPropertyName("filename")] public string? Filename { get; set; }
    [JsonPropertyName("password")] public string? Password { get; set; }
}