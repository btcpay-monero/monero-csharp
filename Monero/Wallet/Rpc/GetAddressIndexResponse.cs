using System.Text.Json.Serialization;

using Monero.Wallet.Common;

namespace Monero.Wallet.Rpc;

public class GetAddressIndexResponse
{
    [JsonPropertyName("index")] public required MoneroSubaddress Index;
}