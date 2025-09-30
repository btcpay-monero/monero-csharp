using System.Text.Json.Serialization;

namespace Monero.Common;

public class MoneroCheck
{
    [JsonPropertyName("good")]
    public bool IsGood { get; set; }
}