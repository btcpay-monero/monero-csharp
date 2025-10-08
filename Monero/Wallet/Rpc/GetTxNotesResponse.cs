using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class GetTxNotesResponse
{
    [JsonPropertyName("notes")] public List<string> Notes { get; set; } = [];
}