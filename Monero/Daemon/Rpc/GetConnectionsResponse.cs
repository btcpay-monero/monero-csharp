using System.Text.Json.Serialization;

using Monero.Daemon.Common;

namespace Monero.Daemon.Rpc;

public class GetConnectionsResponse : MoneroRpcResponse
{
    [JsonPropertyName("connections")] public List<MoneroPeer> Connections { get; set; } = [];
}