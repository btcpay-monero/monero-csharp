using System.Text.Json.Serialization;

using Monero.Daemon.Common;

namespace Monero.Common;

public class MoneroVersion : MoneroRpcResponse
{
    [JsonPropertyName("release")]
    public bool? IsRelease { get; set; }

    [JsonPropertyName("version")]
    public long? Number { get; set; }
}