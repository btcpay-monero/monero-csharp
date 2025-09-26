using System.Text.Json.Serialization;

using Monero.Common;

namespace Monero.Daemon.Rpc;

internal class GetBlockHeadersRangeResult
{
    [JsonPropertyName("headers")] public List<MoneroBlockHeader> Headers { get; set; } = [];
}