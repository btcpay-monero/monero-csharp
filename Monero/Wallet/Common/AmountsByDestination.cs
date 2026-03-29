using System.Text.Json.Serialization;

namespace Monero.Wallet.Common;

public class AmountsByDestination
{
    [JsonPropertyName("amounts")]
    public List<ulong> Amounts { get; set; } = [];
}