using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class GetAddressResponse
{
    [JsonPropertyName("addresses")] public List<RpcBalanceInfo> Addresses { get; set; } = [];
}