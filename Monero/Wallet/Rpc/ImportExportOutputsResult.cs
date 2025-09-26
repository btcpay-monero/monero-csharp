using System.Text.Json.Serialization;

namespace Monero.Wallet.Rpc;

public class ImportExportOutputsResult
{
    [JsonPropertyName("outputs_data_hex")] public string Hex { get; set; } = "";
    [JsonPropertyName("num_imported")] public int NumImported { get; set; }
}