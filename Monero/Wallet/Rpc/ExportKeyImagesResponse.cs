using System.Text.Json.Serialization;

using Monero.Common;

namespace Monero.Wallet.Rpc;

public class ExportKeyImagesResponse
{
    [JsonPropertyName("signed_key_images")]
    public List<MoneroKeyImage> SignedKeyImages { get; set; } = [];
}