using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class GetTxNotesResult
{
    [JsonPropertyName("notes")] public List<string> Notes { get; set; } = [];
}