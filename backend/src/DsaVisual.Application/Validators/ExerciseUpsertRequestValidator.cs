using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence.Entities;
using FluentValidation;

namespace DsaVisual.Application.Validators;

/// <summary>Tạo/sửa bài tập — API_REFERENCE.md §3.7/§4.6.</summary>
public sealed class ExerciseUpsertRequestValidator : AbstractValidator<ExerciseUpsertRequest>
{
    public ExerciseUpsertRequestValidator()
    {
        RuleFor(x => x.LessonId)
            .GreaterThan(0).WithMessage("Bài học không hợp lệ");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề không được để trống")
            .Length(3, 200).WithMessage("Tiêu đề phải từ 3 đến 200 ký tự");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Loại bài tập không hợp lệ");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Trạng thái bài tập không hợp lệ");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).When(x => x.DurationMinutes != null).WithMessage("Thời gian làm bài phải lớn hơn 0");

        RuleFor(x => x.MaxScore)
            .GreaterThanOrEqualTo(0).WithMessage("Điểm tối đa không hợp lệ");

        RuleFor(x => x.Stage)
            .InclusiveBetween(1, 3).When(x => x.Stage != null).WithMessage("Bậc phải từ 1 đến 3");

        RuleFor(x => x.Questions)
            .NotEmpty().When(x => x.Type != ExerciseType.Code).WithMessage("Bài tập cần ít nhất 1 câu hỏi");

        RuleForEach(x => x.Questions).ChildRules(question =>
        {
            question.RuleFor(q => q.Content)
                .NotEmpty().WithMessage("Nội dung câu hỏi không được để trống");

            question.RuleFor(q => q.Points)
                .InclusiveBetween(1, 10).WithMessage("Điểm câu hỏi phải từ 1 đến 10");

            question.RuleFor(q => q.Type)
                .IsInEnum().WithMessage("Loại câu hỏi không hợp lệ");
        });
    }
}
