using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class GetHeightResult
{
    [JsonPropertyName("height")]
    public ulong Height { get; set; }
}