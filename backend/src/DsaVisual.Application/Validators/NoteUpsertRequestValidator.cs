using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Lưu ghi chú bài học — PUT /me/notes/{lessonId} (API_REFERENCE.md §4.12, finding security#7/#12).
/// Giới hạn độ dài ContentHtml (50 KB — chống DB DoS; nội dung HTML đã sanitize ở Controller).</summary>
public sealed class NoteUpsertRequestValidator : AbstractValidator<NoteUpsertRequest>
{
    public const int MaxContentLength = 50_000;

    public NoteUpsertRequestValidator()
    {
        RuleFor(x => x.ContentHtml)
            .MaximumLength(MaxContentLength).WithMessage($"Nội dung ghi chú không được vượt quá {MaxContentLength / 1000} KB");
    }
}
