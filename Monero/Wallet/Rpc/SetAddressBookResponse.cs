using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class SetAddressBookResponse
{
    [JsonPropertyName("index")]
    public int Index { get; set; }
}