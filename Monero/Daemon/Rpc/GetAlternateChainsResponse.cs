using System.Text.Json.Serialization;

using Monero.Daemon.Common;

namespace Monero.Daemon.Rpc;

public class GetAlternateChainsResponse : MoneroRpcResponse
{
    [JsonPropertyName("chains")] public List<MoneroAltChain> Chains { get; set; } = [];
}