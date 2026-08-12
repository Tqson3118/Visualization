namespace DsaVisual.Application.Dtos;

/// <summary>Ghi chú bài học — GET/PUT/DELETE /me/notes/{lessonId} (API_REFERENCE.md §4.12, FR-2.6).</summary>
public sealed class NoteDto
{
    public int LessonId { get; set; }
    public string ContentHtml { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Lưu ghi chú — PUT /me/notes/{lessonId}.</summary>
public sealed class NoteUpsertRequest
{
    public string ContentHtml { get; set; } = string.Empty;
}
