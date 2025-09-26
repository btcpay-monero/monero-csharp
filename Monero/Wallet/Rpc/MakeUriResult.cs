using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class MakeUriResult
{
    [JsonPropertyName("uri")] public string Uri { get; set; } = "";
}