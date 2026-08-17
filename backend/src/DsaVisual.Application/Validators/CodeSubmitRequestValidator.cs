using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Nộp bài code — POST /exercises/{id}/code-submit (API_REFERENCE.md §4.13, finding security#12).
/// Không giới hạn Score/Passed ở đây — clamp Score ≤ MaxScore do ExerciseService xử lý (security#1).</summary>
public sealed class CodeSubmitRequestValidator : AbstractValidator<CodeSubmitRequest>
{
    public const int MaxCodeLength = 200_000;

    public CodeSubmitRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã nguồn không được để trống")
            .MaximumLength(MaxCodeLength).WithMessage($"Mã nguồn không được vượt quá {MaxCodeLength / 1000} KB");

        RuleFor(x => x.Score)
            .GreaterThanOrEqualTo(0).WithMessage("Điểm không hợp lệ");

        RuleFor(x => x.Passed)
            .GreaterThanOrEqualTo(0).WithMessage("Số bài đạt không hợp lệ");

        RuleFor(x => x.Total)
            .GreaterThanOrEqualTo(0).WithMessage("Tổng số bài không hợp lệ");

        RuleFor(x => x.ClientRequestId)
            .MaximumLength(64).WithMessage("ClientRequestId không được vượt quá 64 ký tự")
            .When(x => x.ClientRequestId is not null);

        RuleFor(x => x.TaskId)
            .MaximumLength(128).WithMessage("TaskId không được vượt quá 128 ký tự")
            .When(x => x.TaskId is not null);
    }
}
