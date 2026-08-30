namespace DsaVisual.Application.Dtos;

/// <summary>Câu hỏi trong ExerciseDto — KHÔNG chứa AnswerJson/Explanation (API_REFERENCE.md §3.7, §3.14.1).</summary>
public sealed class QuestionDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = "SINGLE";
    public List<string> Options { get; set; } = [];
    public List<int>? Answer { get; set; }
    public string? Explanation { get; set; }
    public int Points { get; set; } = 1;
}
