using System.Text.Json.Serialization;

using Monero.Common;
using Monero.Daemon.Common;

namespace Monero.Daemon.Rpc;

public class GetLastBlockHeaderResponse : MoneroRpcResponse
{
    [JsonPropertyName("block_header")]
    public required MoneroBlockHeader BlockHeader { get; set; }
}