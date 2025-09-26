using System.Text.Json.Serialization;

using Monero.Wallet.Common;

namespace Monero.Wallet.Rpc;

public class GetAddressBookResult
{
    [JsonPropertyName("entries")] public List<MoneroAddressBookEntry> Entries { get; set; } = [];
}