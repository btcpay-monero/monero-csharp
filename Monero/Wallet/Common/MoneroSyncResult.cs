using System.Text.Json.Serialization;

namespace Monero.Wallet.Common;

public class MoneroSyncResult
{
    [JsonPropertyName("blocks_fetched")]
    public ulong? NumBlocksFetched { get; set; }

    [JsonPropertyName("received_money")]
    public bool? ReceivedMoney { get; set; }
}