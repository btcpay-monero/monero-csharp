using System.Text.Json.Serialization;

using Monero.Daemon.Common;

namespace Monero.Daemon.Rpc;

public class GetAlternateChainsResult : MoneroRpcResponse
{
    [JsonPropertyName("chains")] public List<MoneroAltChain> Chains { get; set; } = [];
}