using DsaVisual.Application.Persistence.Entities;

namespace DsaVisual.Application.Dtos;

/// <summary>Câu hỏi trong ExerciseUpsertRequest — API_REFERENCE.md §4.6 (type SINGLE/MULTI/BOOLEAN/LAB).</summary>
public sealed class QuestionUpsertDto
{
    public string Content { get; set; } = string.Empty;
    public QuestionType Type { get; set; } = QuestionType.Single;
    public List<string> Options { get; set; } = [];
    public string AnswerJson { get; set; } = "[]";     // SINGLE [1]; MULTI [0,2]; BOOLEAN [1]; LAB {type:"STATE_MATCH", finalState, maxSteps}
    public string? Explanation { get; set; }
    public string? Hint1 { get; set; }
    public string? Hint2 { get; set; }
    public string? Hint3 { get; set; }
    public bool KeepOrder { get; set; }
    public int Points { get; set; } = 1;               // 1-10
    public int SortOrder { get; set; }
}
