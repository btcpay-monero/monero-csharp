using System.Text.Json.Serialization;

using Monero.Daemon.Common;

namespace Monero.Common;

public class MoneroJsonRpcResponse<T>
{
    [JsonPropertyName("error")]
    public MoneroRpcResponseError? Error { get; set; }

    [JsonPropertyName("result")]
    public T? Result { get; set; }
}