using System.Text.Json.Serialization;

namespace Monero.Wallet.Common;

public class MoneroAccount
{
    [JsonPropertyName("balance")]
    public ulong Balance { get; set; }

    [JsonPropertyName("account_index")]
    public uint? AccountIndex { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; }

    [JsonPropertyName("base_address")]
    public string? PrimaryAddress { get; set; }

    public List<MoneroSubaddress>? Subaddresses;

    [JsonPropertyName("tag")]
    public string? Tag { get; set; }

    [JsonPropertyName("unlocked_balance")]
    public ulong UnlockedBalance { get; set; }

    public MoneroAccount(uint? accountIndex, string? primaryAddress)
    {
        AccountIndex = accountIndex;
        Label = "";
        PrimaryAddress = primaryAddress;
        Balance = 0;
        UnlockedBalance = 0;
    }
}