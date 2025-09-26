using System.Text.Json.Serialization;

using Monero.Daemon.Common;

namespace Monero.Daemon.Rpc;

public class LimitResult : MoneroRpcResponse
{
    [JsonPropertyName("limit_up")]
    public int LimitUp { get; set; }

    [JsonPropertyName("limit_down")]
    public int LimitDown { get; set; }
}