using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroTxPoolStats : MoneroRpcResponse
{
    [JsonPropertyName("bytes_max")]
    public ulong? BytesMax { get; set; }

    [JsonPropertyName("bytes_med")]
    public ulong? BytesMed { get; set; }

    [JsonPropertyName("bytes_min")]
    public ulong? BytesMin { get; set; }

    [JsonPropertyName("bytes_total")]
    public ulong? BytesTotal { get; set; }

    [JsonPropertyName("fee_total")]
    public ulong? FeeTotal { get; set; }

    [JsonPropertyName("histo")]
    public Dictionary<ulong, int>? Histo { get; set; }

    [JsonPropertyName("histo_98pc")]
    public ulong? Histo98Pc { get; set; }

    [JsonPropertyName("num_10m")]
    public int? Num10M { get; set; }

    [JsonPropertyName("num_double_spends")]
    public int? NumDoubleSpends { get; set; }

    [JsonPropertyName("num_failing")]
    public int? NumFailing { get; set; }

    [JsonPropertyName("num_not_relayed")]
    public int? NumNotRelayed { get; set; }

    [JsonPropertyName("txs_total")]
    public int? NumTxs { get; set; }

    [JsonPropertyName("oldest")]
    public ulong? OldestTimestamp { get; set; }
}