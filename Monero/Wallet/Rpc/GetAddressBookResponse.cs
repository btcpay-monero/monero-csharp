using System.Text.Json.Serialization;

using Monero.Wallet.Common;

namespace Monero.Wallet.Rpc;

public class GetAddressBookResponse
{
    [JsonPropertyName("entries")] public List<MoneroAddressBookEntry> Entries { get; set; } = [];
}