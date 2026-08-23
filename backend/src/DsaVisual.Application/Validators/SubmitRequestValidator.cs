using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Nộp bài — API_REFERENCE.md §3.8/§4.6.</summary>
public sealed class SubmitRequestValidator : AbstractValidator<SubmitRequest>
{
    public SubmitRequestValidator()
    {
        RuleFor(x => x.Answers)
            .NotEmpty().WithMessage("Chưa có câu trả lời nào");

        RuleForEach(x => x.Answers).ChildRules(answer =>
        {
            answer.RuleFor(a => a.QuestionId)
                .GreaterThan(0).WithMessage("Câu hỏi không hợp lệ");

            answer.RuleFor(a => a.Selected)
                .NotNull().WithMessage("Thiếu lựa chọn");

            answer.RuleFor(a => a.LabAnswer)
                .Must(lab => lab is null || lab.StepsUsed >= 0)
                .WithMessage("Số bước không hợp lệ");
        });
    }
}
