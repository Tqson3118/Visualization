using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VisualizationDSA.Domain.Entities;
using VisualizationDSA.Domain.Enums;
using VisualizationDSA.Infrastructure.Data;
using VisualizationDSA.Infrastructure.Services;
using Xunit;

namespace VisualizationDSA.UnitTests.Services
{
    public class RoadmapReviewServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly RoadmapReviewService _service;
        private readonly User _teacher;
        private readonly User _student;
        private readonly CustomRoadmap _roadmap;

        public RoadmapReviewServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);

            _teacher = new User("teacher@dsa.com", "Teacher", "hash");
            _student = new User("student@dsa.com", "Student", "hash");
            _dbContext.Users.AddRange(_teacher, _student);

            _roadmap = new CustomRoadmap(_teacher.Id, "Sorting Roadmap", "desc", "[]", null, "Public");
            _roadmap.Approve();
            _dbContext.CustomRoadmaps.Add(_roadmap);

            _dbContext.SaveChanges();
            _service = new RoadmapReviewService(_dbContext);
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        private RoadmapEnrollment AddEnrollment(Guid userId, string status)
        {
            var enrollment = new RoadmapEnrollment(userId, _roadmap.Id);
            if (status == "Completed") enrollment.MarkCompleted();
            _dbContext.RoadmapEnrollments.Add(enrollment);
            _dbContext.SaveChanges();
            return enrollment;
        }

        // Node-as-lesson: roadmap node được học trực tiếp (luồng Thư viện Lộ trình — node.Id là lessonId).
        private CustomNode AddNode(string name, int sortOrder, Guid? labId = null)
        {
            var node = new CustomNode(_roadmap.Id, name, "desc", "Easy", sortOrder);
            if (labId.HasValue) node.UpdatePractice(null, labId.Value, null);
            _dbContext.CustomNodes.Add(node);
            _dbContext.SaveChanges();
            return node;
        }

        // Node có LabId liên kết một lesson thật qua ModuleItem (mapping giống GetMyEnrollments).
        private CustomNode AddNodeWithLabLesson(string name, int sortOrder, out Guid lessonId)
        {
            var lesson = new Lesson($"Lesson {name}", "content", "", "[]", 30);
            _dbContext.Lessons.Add(lesson);
            var codelab = new Codelab($"Lab {name}", "desc", "code", 1, 30);
            _dbContext.Codelabs.Add(codelab);
            var module = new CourseModule(Guid.NewGuid(), "M", "desc", 1);
            _dbContext.CourseModules.Add(module);
            var item = new ModuleItem(module.Id, null, ModuleItemType.Codelab, null, null, codelab.Id, $"Lab {name}", 1, true);
            // Trong DB thực, item này có thể mang thêm LessonId (legacy mapping) dù constructor cấm — mô phỏng qua reflection.
            var lessonIdProp = typeof(ModuleItem).GetProperty("LessonId");
            lessonIdProp?.SetValue(item, lesson.Id);
            _dbContext.ModuleItems.Add(item);

            var node = new CustomNode(_roadmap.Id, name, "desc", "Easy", sortOrder);
            node.UpdatePractice(null, codelab.Id, null);
            _dbContext.CustomNodes.Add(node);
            _dbContext.SaveChanges();
            lessonId = lesson.Id;
            return node;
        }

        private void MarkLessonCompleted(Guid userId, Guid lessonId)
        {
            var progress = new UserLessonProgress(userId, lessonId, "Completed");
            _dbContext.UserLessonProgresses.Add(progress);
            _dbContext.SaveChanges();
        }

        // ── SubmitReview ──

        [Fact]
        public async Task SubmitReview_WhenEnrollmentCompleted_CreatesReview()
        {
            AddEnrollment(_student.Id, "Completed");

            var result = await _service.SubmitReviewAsync(_student.Id, _roadmap.Id, 5);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Review);
            Assert.Equal(5, result.Review!.Rating);
            Assert.Equal(_roadmap.Id, result.Review.RoadmapId);

            var saved = await _dbContext.RoadmapReviews.FirstAsync(r => r.RoadmapId == _roadmap.Id);
            Assert.Equal(_student.Id, saved.UserId);
        }

        [Fact]
        public async Task SubmitReview_AlreadyReviewed_ReturnsAlreadyReviewed_WithoutUpdate()
        {
            AddEnrollment(_student.Id, "Completed");
            _dbContext.RoadmapReviews.Add(new RoadmapReview(_student.Id, _roadmap.Id, 4));
            _dbContext.SaveChanges();

            var result = await _service.SubmitReviewAsync(_student.Id, _roadmap.Id, 2);

            Assert.False(result.IsSuccess);
            Assert.Equal("ALREADY_REVIEWED", result.ErrorCode);

            var reviews = _dbContext.RoadmapReviews.Where(r => r.RoadmapId == _roadmap.Id).ToList();
            Assert.Single(reviews);
            Assert.Equal(4, reviews[0].Rating);
        }

        [Fact]
        public async Task SubmitReview_NotCompleted_ReturnsNotCompleted()
        {
            AddEnrollment(_student.Id, "Active");

            var result = await _service.SubmitReviewAsync(_student.Id, _roadmap.Id, 5);

            Assert.False(result.IsSuccess);
            Assert.Equal("ROADMAP_NOT_COMPLETED", result.ErrorCode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        [InlineData(-1)]
        public async Task SubmitReview_InvalidRating_ReturnsInvalidRating(int rating)
        {
            var result = await _service.SubmitReviewAsync(_student.Id, _roadmap.Id, rating);

            Assert.False(result.IsSuccess);
            Assert.Equal("INVALID_RATING", result.ErrorCode);
        }

        [Fact]
        public async Task SubmitReview_RoadmapNotFound_ReturnsNotFound()
        {
            AddEnrollment(_student.Id, "Completed");

            var result = await _service.SubmitReviewAsync(_student.Id, Guid.NewGuid(), 5);

            Assert.False(result.IsSuccess);
            Assert.Equal("ROADMAP_NOT_FOUND", result.ErrorCode);
        }

        // ── MarkRoadmapCompletedIfLastLessonAsync ──

        [Fact]
        public async Task MarkRoadmapCompleted_LastLessonDone_MarksEnrollmentCompleted()
        {
            AddNode("Node1", 1);
            var lastNode = AddNode("Node2", 2);
            AddEnrollment(_student.Id, "Active");

            await _service.MarkRoadmapCompletedIfLastLessonAsync(_student.Id, lastNode.Id);

            var enrollment = await _dbContext.RoadmapEnrollments.FirstAsync(e => e.UserId == _student.Id);
            Assert.Equal("Completed", enrollment.Status);
            Assert.NotNull(enrollment.CompletedAt);
        }

        [Fact]
        public async Task MarkRoadmapCompleted_NotLastLesson_DoesNotMarkCompleted()
        {
            var firstNode = AddNode("Node1", 1);
            AddNode("Node2", 2);
            AddEnrollment(_student.Id, "Active");

            await _service.MarkRoadmapCompletedIfLastLessonAsync(_student.Id, firstNode.Id);

            var enrollment = await _dbContext.RoadmapEnrollments.FirstAsync(e => e.UserId == _student.Id);
            Assert.Equal("Active", enrollment.Status);
        }

        [Fact]
        public async Task MarkRoadmapCompleted_NodeWithLabLessonNotDone_DoesNotMarkCompleted()
        {
            // Node có LabId liên kết lesson thật — lesson CHƯA Completed → không đánh dấu nhầm
            AddNodeWithLabLesson("Node1", 1, out _);
            var lastNode = AddNodeWithLabLesson("Node2", 2, out _);
            AddEnrollment(_student.Id, "Active");

            await _service.MarkRoadmapCompletedIfLastLessonAsync(_student.Id, lastNode.Id);

            var enrollment = await _dbContext.RoadmapEnrollments.FirstAsync(e => e.UserId == _student.Id);
            Assert.Equal("Active", enrollment.Status);
        }

        [Fact]
        public async Task MarkRoadmapCompleted_AllLabLessonsDone_MarksCompleted()
        {
            // Mọi node Lab-linked đã Completed → hoàn thành node cuối → enrollment Completed
            AddNodeWithLabLesson("Node1", 1, out var lesson1Id);
            var lastNode = AddNodeWithLabLesson("Node2", 2, out var lesson2Id);
            AddEnrollment(_student.Id, "Active");
            MarkLessonCompleted(_student.Id, lesson1Id);
            MarkLessonCompleted(_student.Id, lesson2Id);

            await _service.MarkRoadmapCompletedIfLastLessonAsync(_student.Id, lastNode.Id);

            var enrollment = await _dbContext.RoadmapEnrollments.FirstAsync(e => e.UserId == _student.Id);
            Assert.Equal("Completed", enrollment.Status);
        }

        [Fact]
        public async Task MarkRoadmapCompleted_EnrollmentAlreadyCompleted_DoesNotChange()
        {
            AddNode("Node1", 1);
            var lastNode = AddNode("Node2", 2);
            var enrollment = AddEnrollment(_student.Id, "Completed");

            await _service.MarkRoadmapCompletedIfLastLessonAsync(_student.Id, lastNode.Id);

            var loaded = await _dbContext.RoadmapEnrollments.FirstAsync(e => e.Id == enrollment.Id);
            Assert.Equal("Completed", loaded.Status);
        }

        // ── GetStats ──

        [Fact]
        public async Task GetStats_ReturnsCountsAndAverageRating()
        {
            AddEnrollment(_student.Id, "Completed");
            AddEnrollment(Guid.NewGuid(), "Active");
            AddEnrollment(Guid.NewGuid(), "Active");

            _dbContext.RoadmapReviews.Add(new RoadmapReview(Guid.NewGuid(), _roadmap.Id, 5));
            _dbContext.RoadmapReviews.Add(new RoadmapReview(Guid.NewGuid(), _roadmap.Id, 4));
            _dbContext.SaveChanges();

            var stats = await _service.GetStatsAsync(_roadmap.Id, null);

            Assert.Equal(3, stats.EnrollCount);
            Assert.Equal(1, stats.CompletionCount);
            Assert.Equal(2, stats.ReviewCount);
            Assert.Equal(4.5, stats.AvgRating);
            Assert.Null(stats.MyRating);
            Assert.False(stats.MyCanReview);
        }

        [Fact]
        public async Task GetStats_NoReviews_AvgRatingNull()
        {
            AddEnrollment(_student.Id, "Active");

            var stats = await _service.GetStatsAsync(_roadmap.Id, null);

            Assert.Equal(1, stats.EnrollCount);
            Assert.Equal(0, stats.CompletionCount);
            Assert.Equal(0, stats.ReviewCount);
            Assert.Null(stats.AvgRating);
        }

        [Fact]
        public async Task GetStats_MyCanReview_TrueWhenCompletedAndNotReviewed()
        {
            AddEnrollment(_student.Id, "Completed");

            var stats = await _service.GetStatsAsync(_roadmap.Id, _student.Id);

            Assert.Null(stats.MyRating);
            Assert.True(stats.MyCanReview);
        }

        [Fact]
        public async Task GetStats_MyRatingSet_AfterReview()
        {
            AddEnrollment(_student.Id, "Completed");
            _dbContext.RoadmapReviews.Add(new RoadmapReview(_student.Id, _roadmap.Id, 3));
            _dbContext.SaveChanges();

            var stats = await _service.GetStatsAsync(_roadmap.Id, _student.Id);

            Assert.Equal(3, stats.MyRating);
            Assert.False(stats.MyCanReview);
        }

        [Fact]
        public async Task RoadmapExists_ReturnsTrue_ForExistingRoadmap()
        {
            Assert.True(await _service.RoadmapExistsAsync(_roadmap.Id));
            Assert.False(await _service.RoadmapExistsAsync(Guid.NewGuid()));
        }
    }
}
