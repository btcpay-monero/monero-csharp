using System.Text.Json.Serialization;

using Monero.Wallet.Common;

namespace Monero.Wallet.Rpc;

public class GetAddressIndexResult
{
    [JsonPropertyName("index")] public required MoneroSubaddress Index;
}