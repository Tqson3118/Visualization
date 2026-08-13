using DsaVisual.Application.Dtos;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Chạy benchmark — POST /benchmarks/run (API_REFERENCE.md §4.14, finding security#12).</summary>
public sealed class BenchmarkRequestValidator : AbstractValidator<BenchmarkRequest>
{
    public BenchmarkRequestValidator()
    {
        RuleFor(x => x.Keys)
            .NotEmpty().WithMessage("Thiếu keys cho benchmark")
            .Must(keys => keys.Count <= 50).WithMessage("Quá nhiều keys cho benchmark");

        RuleFor(x => x.Sizes)
            .NotEmpty().WithMessage("Thiếu sizes cho benchmark")
            .Must(sizes => sizes.Count <= 200).WithMessage("Quá nhiều sizes cho benchmark");

        RuleFor(x => x.Results)
            .NotEmpty().WithMessage("Thiếu kết quả đo cho benchmark")
            .When(x => x.Results is not null);

        RuleFor(x => x.Language)
            .MaximumLength(20).WithMessage("Ngôn ngữ không hợp lệ")
            .When(x => x.Language is not null);
    }
}
