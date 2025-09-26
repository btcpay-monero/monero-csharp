using System.Text.Json.Serialization;

using Monero.Daemon.Common;

namespace Monero.Daemon.Rpc;

public class MoneroPeerInfo
{
    [JsonPropertyName("info")]
    public MoneroPeer? Info { get; set; }
}