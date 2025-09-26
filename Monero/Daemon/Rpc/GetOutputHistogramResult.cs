using System.Text.Json.Serialization;

using Monero.Daemon.Common;

namespace Monero.Daemon.Rpc;

public class GetOutputHistogramResult : MoneroRpcResponse
{
    [JsonPropertyName("histogram")] public List<MoneroOutputHistogramEntry> Histogram { get; set; } = [];
}