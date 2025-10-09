using System.Text.Json.Serialization;

namespace Monero.Wallet.Common;

public class TransferDestination
{

    [JsonPropertyName("address")]
    public required string Address { get; set; }

    [JsonPropertyName("amount")]
    public long Amount { get; set; }
}