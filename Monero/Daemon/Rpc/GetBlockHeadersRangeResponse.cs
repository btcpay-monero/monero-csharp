using System.Text.Json.Serialization;

using Monero.Common;

namespace Monero.Daemon.Rpc;

internal class GetBlockHeadersRangeResponse
{
    [JsonPropertyName("headers")] public List<MoneroBlockHeader> Headers { get; set; } = [];
}