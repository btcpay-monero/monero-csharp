using System.Text.Json.Serialization;

using Monero.Wallet.Common;

namespace Monero.Wallet.Rpc;

public class GetTransferByTxIdResult
{
    [JsonPropertyName("transfer")]
    public MoneroTransfer? Transfer { get; set; }

    [JsonPropertyName("transfers")]
    public List<MoneroTransfer> Transfers { get; set; } = [];
}