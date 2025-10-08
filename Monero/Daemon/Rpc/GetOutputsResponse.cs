using System.Text.Json.Serialization;

using Monero.Common;
using Monero.Daemon.Common;

namespace Monero.Daemon.Rpc;

public class GetOutputsResponse : MoneroRpcResponse
{
    [JsonPropertyName("outs")] public List<MoneroOutput> Outs { get; set; } = [];
}