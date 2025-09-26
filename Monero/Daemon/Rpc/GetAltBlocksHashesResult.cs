using System.Text.Json.Serialization;

using Monero.Daemon.Common;

namespace Monero.Daemon.Rpc;

public class GetAltBlocksHashesResult : MoneroRpcResponse
{
    [JsonPropertyName("blks_hashes")] public List<string> BlockHashes { get; set; } = [];
}