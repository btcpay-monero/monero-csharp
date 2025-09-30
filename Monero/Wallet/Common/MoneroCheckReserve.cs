using System.Text.Json.Serialization;

using Monero.Common;

namespace Monero.Wallet.Common;

public class MoneroCheckReserve : MoneroCheck
{
    [JsonPropertyName("total")]
    public ulong? TotalAmount { get; set; }

    [JsonPropertyName("spent")]
    public ulong? UnconfirmedSpentAmount { get; set; }
}