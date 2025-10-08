using System.Text.Json.Serialization;

using Monero.Daemon.Common;

namespace Monero.Daemon.Rpc;

public class IsKeyImageSpentResponse : MoneroRpcResponse
{
    [JsonPropertyName("spent_status")] public List<int> SpentStatus { get; set; } = [];
}