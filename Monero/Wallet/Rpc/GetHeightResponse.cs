using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class GetHeightResponse
{
    [JsonPropertyName("height")]
    public ulong Height { get; set; }
}