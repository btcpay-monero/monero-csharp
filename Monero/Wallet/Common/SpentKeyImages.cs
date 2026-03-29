using System.Text.Json.Serialization;

namespace Monero.Wallet.Common;

public class SpentKeyImages
{
    [JsonPropertyName("key_images")]
    public List<string> KeyImages { get; set; } = [];
}