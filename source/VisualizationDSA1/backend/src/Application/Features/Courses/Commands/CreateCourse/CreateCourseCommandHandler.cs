using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VisualizationDSA.Domain.Entities;
using VisualizationDSA.Domain.Enums;
using VisualizationDSA.Application.Interfaces;

namespace VisualizationDSA.Application.Features.Courses.Commands.CreateCourse
{
    public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public CreateCourseCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
        {
            // Case-insensitive: frontend gửi "sorting"/"DataStructure" lẫn lộn → chuẩn hóa về enum
            var category = Enum.TryParse<CourseCategory>(request.Category, true, out var parsedCategory)
                ? parsedCategory
                : CourseCategory.DataStructure;
            var difficulty = Enum.TryParse<CourseDifficulty>(request.Difficulty, true, out var parsedDifficulty)
                ? parsedDifficulty
                : CourseDifficulty.Beginner;

            var course = new Course(
                request.TeacherId,
                request.Title,
                request.Description,
                category,
                difficulty,
                request.IsPremium,
                request.Thumbnail
            );

            if (request.IsPublished) course.Publish();

            _context.Courses.Add(course);
            await _context.SaveChangesAsync(cancellationToken);

            return course.Id;
        }
    }
}