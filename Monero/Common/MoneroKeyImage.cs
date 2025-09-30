using System.Text.Json.Serialization;

namespace Monero.Common;

public class MoneroKeyImage
{
    public enum SpentStatus
    {
        NotSpent,
        Confirmed,
        TxPool
    }

    [JsonPropertyName("key_image")]
    public string? Hex { get; set; }

    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}