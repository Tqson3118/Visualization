using System.Text.Json;

namespace DsaVisual.Application.Dtos;

/// <summary>Đáp án 1 câu — API_REFERENCE.md §3.8 (selected: int[]; labAnswer cho LAB).</summary>
public sealed class AnswerDto
{
    public int QuestionId { get; set; }
    public List<int> Selected { get; set; } = [];
    public JsonElement? SimAnswer { get; set; }        // dự phòng SIMULATION_PREDICT
    public LabAnswerDto? LabAnswer { get; set; }
}

/// <summary>Đáp án LAB (Bậc 2) — API_REFERENCE.md §4.16: chấm trạng thái cuối + số bước.</summary>
public sealed class LabAnswerDto
{
    public JsonElement FinalState { get; set; }
    public int StepsUsed { get; set; }
    public int MaxSteps { get; set; }
}
