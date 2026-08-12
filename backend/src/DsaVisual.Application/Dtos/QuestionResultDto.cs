using System.Text.Json;

namespace DsaVisual.Application.Dtos;

/// <summary>Kết quả 1 câu — API_REFERENCE.md §3.8.</summary>
public sealed class QuestionResultDto
{
    public int QuestionId { get; set; }
    public bool Correct { get; set; }
    public JsonElement CorrectAnswer { get; set; }
    public string? Explanation { get; set; }
}
