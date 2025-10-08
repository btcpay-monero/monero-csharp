using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class MakeUriResponse
{
    [JsonPropertyName("uri")] public string Uri { get; set; } = "";
}