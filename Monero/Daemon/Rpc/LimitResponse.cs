using System.Text.Json.Serialization;

using Monero.Daemon.Common;

namespace Monero.Daemon.Rpc;

public class LimitResponse : MoneroRpcResponse
{
    [JsonPropertyName("limit_up")]
    public int LimitUp { get; set; }

    [JsonPropertyName("limit_down")]
    public int LimitDown { get; set; }
}