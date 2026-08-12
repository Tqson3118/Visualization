namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Câu hỏi của bài tập — SDD §7.3.3.</summary>
public sealed class Question
{
    public int Id { get; set; }
    public int ExerciseId { get; set; }                              // FK cascade
    public string Content { get; set; } = string.Empty;              // Markdown
    public string OptionsJson { get; set; } = string.Empty;          // ["A","B","C","D"]
    public string AnswerJson { get; set; } = string.Empty;           // SINGLE [1]; MULTI [0,2]; BOOLEAN [1]; Lab: {type:STATE_MATCH,...}
    public string? Explanation { get; set; }
    public string? Hint1 { get; set; }
    public string? Hint2 { get; set; }
    public string? Hint3 { get; set; }
    public string? WrongExplanationsJson { get; set; }               // giải thích từng phương án sai (FR-4.9)
    public bool KeepOrder { get; set; }                              // không xáo trộn phương án (FR-4.8)
    public int Points { get; set; } = 1;                             // 1-10
    public int SortOrder { get; set; }
}
