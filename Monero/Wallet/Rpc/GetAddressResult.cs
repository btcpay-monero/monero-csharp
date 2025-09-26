using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class GetAddressResult
{
    [JsonPropertyName("addresses")] public List<RpcBalanceInfo> Addresses { get; set; } = [];
}