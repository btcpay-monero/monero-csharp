using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class SetAddressBookResult
{
    [JsonPropertyName("index")]
    public int Index { get; set; }
}