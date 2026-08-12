using System.Text.Json;

namespace DsaVisual.Application.Dtos;

/// <summary>Schema đầu vào mô phỏng — GET /simulations/{key}/schema (API_REFERENCE.md §4.5).</summary>
public sealed class SimulationSchemaDto
{
    public string Key { get; set; } = string.Empty;
    public JsonElement Schema { get; set; }
}
