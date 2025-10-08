using System.Text.Json.Serialization;

using Monero.Daemon.Common;

namespace Monero.Daemon.Rpc;

public class GetPeerListResponse : MoneroRpcResponse
{
    [JsonPropertyName("gray_list")] public List<MoneroPeer> GrayList { get; set; } = [];
    [JsonPropertyName("white_list")] public List<MoneroPeer> WhiteList { get; set; } = [];
}