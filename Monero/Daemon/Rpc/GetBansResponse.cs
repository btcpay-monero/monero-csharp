using System.Text.Json.Serialization;

using Monero.Daemon.Common;

namespace Monero.Daemon.Rpc;

public class GetBansResponse : MoneroRpcResponse
{
    [JsonPropertyName("bans")] public List<MoneroBan> Bans { get; set; } = [];
}