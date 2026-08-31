using System.Text.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Services;

public sealed class PathItemService(
    AppDbContext db,
    IDateTimeProvider clock,
    ILogger<PathItemService> logger) : IPathItemService
{
    private const string RoleAdmin = "ADMIN";

    public async Task<Result<List<PathItemDto>>> GetTreeAsync(int userId, string role, int pathId, CancellationToken ct)
    {
        var path = await db.LearningPaths.AsNoTracking().FirstOrDefaultAsync(p => p.Id == pathId, ct);
        if (path is null)
        {
            return Result<List<PathItemDto>>.Fail(ErrorCodes.NOT_FOUND, "Lộ trình học không tồn tại");
        }

        // Phân quyền đọc cây: cho phép nếu là Admin/Author hoặc lộ trình đã Active/Published
        if (!CanViewPath(userId, role, path))
        {
            return Result<List<PathItemDto>>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền xem lộ trình này");
        }

        var nodes = await db.LearningPathNodes.AsNoTracking()
            .Where(n => n.PathId == pathId)
            .OrderBy(n => n.SortOrder)
            .ToListAsync(ct);

        var nodeDtos = nodes.Select(MapToDto).ToList();
        var tree = BuildHierarchy(nodeDtos);

        return Result<List<PathItemDto>>.Ok(tree);
    }

    public async Task<Result<PathItemDto>> GetItemDetailAsync(int userId, string role, int itemId, CancellationToken ct)
    {
        var node = await db.LearningPathNodes.FirstOrDefaultAsync(n => n.Id == itemId, ct);
        if (node is null)
        {
            return Result<PathItemDto>.Fail(ErrorCodes.NOT_FOUND, "Mục lộ trình không tồn tại");
        }

        var path = await db.LearningPaths.AsNoTracking().FirstOrDefaultAsync(p => p.Id == node.PathId, ct);
        if (path is null)
        {
            return Result<PathItemDto>.Fail(ErrorCodes.NOT_FOUND, "Lộ trình học không tồn tại");
        }

        // Phân quyền đọc chi tiết mục
        if (!CanViewPath(userId, role, path))
        {
            return Result<PathItemDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền xem mục lộ trình này");
        }

        var dto = MapToDto(node);

        if (node.ItemType == PathItemType.Theory && node.LessonId is { } lessonId)
        {
            var lesson = await db.Lessons.AsNoTracking()
                .Include(l => l.LessonSimulations)
                .FirstOrDefaultAsync(l => l.Id == lessonId, ct);
            if (lesson is not null)
            {
                dto.Lesson = new LessonDto
                {
                    Id = lesson.Id,
                    TopicId = lesson.TopicId,
                    Title = lesson.Title,
                    Description = lesson.Description,
                    ContentHtml = lesson.ContentHtml,
                    Status = lesson.Status.ToString(),
                    IsClassOnly = lesson.IsClassOnly,
                    SortOrder = lesson.SortOrder,
                    Simulations = lesson.LessonSimulations
                        .OrderBy(s => s.SortOrder)
                        .Select(s => new SimulationRefDto { SimulationKey = s.SimulationKey, Title = s.Title })
                        .ToList()
                };
            }
        }
        else if ((node.ItemType == PathItemType.Quiz || node.ItemType == PathItemType.Lab) && (node.FinalTestId ?? node.LabExerciseId) is { } exId)
        {
            var exercise = await db.Exercises.AsNoTracking()
                .Include(e => e.Questions)
                .FirstOrDefaultAsync(e => e.Id == exId && e.DeletedAt == null, ct);
            if (exercise is not null)
            {
                dto.Exercise = new ExerciseDto
                {
                    Id = exercise.Id,
                    LessonId = exercise.LessonId,
                    NodeId = exercise.NodeId,
                    Stage = exercise.Stage,
                    Title = exercise.Title,
                    Description = exercise.Description,
                    Type = ExerciseService.ToExerciseTypeString(exercise.Type),
                    ConfigJson = exercise.ConfigJson,
                    DurationMinutes = exercise.DurationMinutes,
                    MaxScore = exercise.MaxScore,
                    Status = exercise.Status.ToString(),
                    Questions = exercise.Questions
                        .OrderBy(q => q.SortOrder)
                        .Select(q => new QuestionDto
                        {
                            Id = q.Id,
                            Content = q.Content,
                            Type = q.Type.ToString().ToUpperInvariant(),
                            Options = DeserializeOptions(q.OptionsJson),
                            Answer = DeserializeAnswer(q.AnswerJson),
                            Explanation = q.Explanation,
                            Points = q.Points
                        })
                        .ToList()
                };
            }
        }

        return Result<PathItemDto>.Ok(dto);
    }

    public async Task<Result<PathItemDto>> CreateItemAsync(int userId, string role, int pathId, PathItemCreateRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Result<PathItemDto>.Fail(ErrorCodes.VALIDATION_FAILED, "Tiêu đề không được để trống");
        }

        var path = await db.LearningPaths.FirstOrDefaultAsync(p => p.Id == pathId, ct);
        if (path is null)
        {
            return Result<PathItemDto>.Fail(ErrorCodes.NOT_FOUND, "Lộ trình học không tồn tại");
        }

        if (!CanManagePath(userId, role, path))
        {
            return Result<PathItemDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền chỉnh sửa lộ trình này");
        }

        if (request.ParentId is { } pId)
        {
            var parent = await db.LearningPathNodes.FirstOrDefaultAsync(n => n.Id == pId && n.PathId == pathId, ct);
            if (parent is null)
            {
                return Result<PathItemDto>.Fail(ErrorCodes.NOT_FOUND, "Thư mục cha không tồn tại");
            }
            if (parent.ItemType != PathItemType.Folder)
            {
                return Result<PathItemDto>.Fail(ErrorCodes.VALIDATION_FAILED, "Chỉ có thể tạo mục con bên trong Thư mục (Folder)");
            }
        }

        var siblings = await db.LearningPathNodes
            .Where(n => n.PathId == pathId && n.ParentId == request.ParentId)
            .ToListAsync(ct);
        var nextSortOrder = request.SortOrder ?? (siblings.Count > 0 ? siblings.Max(s => s.SortOrder) + 1 : 1);

        var now = clock.UtcNow;
        var defaultTopic = await db.Topics.OrderBy(t => t.Id).FirstOrDefaultAsync(ct);
        var topicId = path.TopicId ?? defaultTopic?.Id ?? 1;

        var node = new LearningPathNode
        {
            PathId = pathId,
            ParentId = request.ParentId,
            ItemType = request.ItemType,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            SortOrder = nextSortOrder
        };

        Exercise? createdExercise = null;
        Lesson? createdLesson = null;

        switch (request.ItemType)
        {
            case PathItemType.Folder:
                node.LessonId = null;
                node.FinalTestId = null;
                node.LabExerciseId = null;
                break;

            case PathItemType.Theory:
                if (request.LessonId is { } linkedLessonId)
                {
                    var linkedLesson = await db.Lessons.FirstOrDefaultAsync(l => l.Id == linkedLessonId && l.DeletedAt == null, ct);
                    if (linkedLesson is null)
                    {
                        return Result<PathItemDto>.Fail(ErrorCodes.NOT_FOUND, "Bài học được chọn không tồn tại");
                    }
                    node.LessonId = linkedLesson.Id;
                    createdLesson = linkedLesson;
                }
                else
                {
                    var lesson = new Lesson
                    {
                        TopicId = topicId,
                        Title = request.Title.Trim(),
                        Description = request.Description?.Trim(),
                        ContentHtml = "",
                        Status = LessonStatus.Active,
                        CreatedBy = userId,
                        CreatedAt = now
                    };
                    db.Lessons.Add(lesson);
                    await db.SaveChangesAsync(ct);
                    node.LessonId = lesson.Id;
                    createdLesson = lesson;
                }
                break;

            case PathItemType.Quiz:
                var quizContainerLesson = new Lesson
                {
                    TopicId = topicId,
                    Title = request.Title.Trim(),
                    Description = request.Description?.Trim(),
                    ContentHtml = "",
                    Status = LessonStatus.Hidden,
                    CreatedBy = userId,
                    CreatedAt = now
                };
                db.Lessons.Add(quizContainerLesson);
                await db.SaveChangesAsync(ct);

                createdExercise = new Exercise
                {
                    LessonId = quizContainerLesson.Id,
                    Title = request.Title.Trim(),
                    Description = request.Description?.Trim() ?? "Trắc nghiệm củng cố kiến thức.",
                    Type = ExerciseType.Mcq,
                    MaxScore = 0,
                    Status = ExerciseStatus.Active,
                    CreatedBy = userId,
                    CreatedAt = now
                };
                db.Exercises.Add(createdExercise);
                await db.SaveChangesAsync(ct);

                node.FinalTestId = createdExercise.Id;
                break;

            case PathItemType.Lab:
                var labContainerLesson = new Lesson
                {
                    TopicId = topicId,
                    Title = request.Title.Trim(),
                    Description = request.Description?.Trim(),
                    ContentHtml = "",
                    Status = LessonStatus.Hidden,
                    CreatedBy = userId,
                    CreatedAt = now
                };
                db.Lessons.Add(labContainerLesson);
                await db.SaveChangesAsync(ct);

                createdExercise = new Exercise
                {
                    LessonId = labContainerLesson.Id,
                    Title = request.Title.Trim(),
                    Description = request.Description?.Trim() ?? "Thử thách lập trình.",
                    Type = ExerciseType.Code,
                    Stage = 3,
                    MaxScore = 100,
                    Status = ExerciseStatus.Active,
                    CreatedBy = userId,
                    CreatedAt = now
                };
                db.Exercises.Add(createdExercise);
                await db.SaveChangesAsync(ct);

                node.LabExerciseId = createdExercise.Id;
                break;
        }

        db.LearningPathNodes.Add(node);
        await db.SaveChangesAsync(ct);

        if (createdExercise is not null)
        {
            createdExercise.NodeId = node.Id;
            await db.SaveChangesAsync(ct);
        }

        var resultDto = MapToDto(node);
        if (createdExercise is not null)
        {
            resultDto.Exercise = new ExerciseDto
            {
                Id = createdExercise.Id,
                LessonId = createdExercise.LessonId,
                NodeId = createdExercise.NodeId,
                Stage = createdExercise.Stage,
                Title = createdExercise.Title,
                Description = createdExercise.Description,
                Type = ExerciseService.ToExerciseTypeString(createdExercise.Type),
                DurationMinutes = createdExercise.DurationMinutes,
                MaxScore = createdExercise.MaxScore,
                Status = createdExercise.Status.ToString(),
                Questions = []
            };
        }
        else if (createdLesson is not null)
        {
            resultDto.Lesson = new LessonDto
            {
                Id = createdLesson.Id,
                TopicId = createdLesson.TopicId,
                Title = createdLesson.Title,
                Description = createdLesson.Description,
                ContentHtml = createdLesson.ContentHtml,
                Status = createdLesson.Status.ToString(),
                IsClassOnly = createdLesson.IsClassOnly,
                SortOrder = createdLesson.SortOrder
            };
        }

        logger.LogInformation("PathItem {NodeId} ({Type}: {Title}) created in Path {PathId} by user {UserId}",
            node.Id, node.ItemType, node.Title, pathId, userId);

        return Result<PathItemDto>.Ok(resultDto);
    }

    public async Task<Result<PathItemDto>> UpdateItemAsync(int userId, string role, int itemId, PathItemUpdateRequest request, CancellationToken ct)
    {
        var node = await db.LearningPathNodes.FirstOrDefaultAsync(n => n.Id == itemId, ct);
        if (node is null)
        {
            return Result<PathItemDto>.Fail(ErrorCodes.NOT_FOUND, "Mục lộ trình không tồn tại");
        }

        var path = await db.LearningPaths.FirstOrDefaultAsync(p => p.Id == node.PathId, ct);
        if (path is null || !CanManagePath(userId, role, path))
        {
            return Result<PathItemDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền chỉnh sửa mục này");
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            node.Title = request.Title.Trim();

            // Đồng bộ tiêu đề với Lesson/Exercise liên quan
            if (node.LessonId is { } lessonId)
            {
                var lesson = await db.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId, ct);
                if (lesson is not null && lesson.Status == LessonStatus.Hidden)
                {
                    lesson.Title = node.Title;
                }
            }

            var exId = node.FinalTestId ?? node.LabExerciseId;
            if (exId is { } exerciseId)
            {
                var exercise = await db.Exercises.FirstOrDefaultAsync(e => e.Id == exerciseId && e.DeletedAt == null, ct);
                if (exercise is not null)
                {
                    exercise.Title = node.Title;
                }
            }
        }

        if (request.Description is not null)
        {
            node.Description = request.Description.Trim();
        }

        if (request.LessonId.HasValue)
        {
            node.LessonId = request.LessonId.Value;
        }

        if (request.FinalTestId.HasValue)
        {
            node.FinalTestId = request.FinalTestId.Value;
        }

        if (request.LabExerciseId.HasValue)
        {
            node.LabExerciseId = request.LabExerciseId.Value;
        }

        await db.SaveChangesAsync(ct);
        return Result<PathItemDto>.Ok(MapToDto(node));
    }

    public async Task<Result<PathItemDto>> MoveItemAsync(int userId, string role, int itemId, PathItemMoveRequest request, CancellationToken ct)
    {
        var node = await db.LearningPathNodes.FirstOrDefaultAsync(n => n.Id == itemId, ct);
        if (node is null)
        {
            return Result<PathItemDto>.Fail(ErrorCodes.NOT_FOUND, "Mục lộ trình không tồn tại");
        }

        var path = await db.LearningPaths.FirstOrDefaultAsync(p => p.Id == node.PathId, ct);
        if (path is null || !CanManagePath(userId, role, path))
        {
            return Result<PathItemDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền di chuyển mục này");
        }

        if (request.ParentId is { } pId)
        {
            if (pId == node.Id)
            {
                return Result<PathItemDto>.Fail(ErrorCodes.VALIDATION_FAILED, "Không thể di chuyển một mục vào chính nó");
            }

            var parent = await db.LearningPathNodes.FirstOrDefaultAsync(n => n.Id == pId && n.PathId == node.PathId, ct);
            if (parent is null)
            {
                return Result<PathItemDto>.Fail(ErrorCodes.NOT_FOUND, "Thư mục đích không tồn tại");
            }
            if (parent.ItemType != PathItemType.Folder)
            {
                return Result<PathItemDto>.Fail(ErrorCodes.VALIDATION_FAILED, "Mục cha phải là Thư mục (Folder)");
            }

            // Chống cycle: parent không được là hậu duệ của node
            var allPathNodes = await db.LearningPathNodes.AsNoTracking().Where(n => n.PathId == node.PathId).ToListAsync(ct);
            var nodeMap = allPathNodes.ToDictionary(n => n.Id);
            var curr = parent;
            while (curr.ParentId is { } ancestorId)
            {
                if (ancestorId == node.Id)
                {
                    return Result<PathItemDto>.Fail(ErrorCodes.VALIDATION_FAILED, "Không thể di chuyển thư mục vào thư mục con của nó");
                }
                if (!nodeMap.TryGetValue(ancestorId, out curr!))
                {
                    break;
                }
            }
        }

        node.ParentId = request.ParentId;
        node.SortOrder = request.SortOrder;
        await db.SaveChangesAsync(ct);

        return Result<PathItemDto>.Ok(MapToDto(node));
    }

    public async Task<Result> DeleteItemAsync(int userId, string role, int itemId, CancellationToken ct)
    {
        var node = await db.LearningPathNodes.FirstOrDefaultAsync(n => n.Id == itemId, ct);
        if (node is null)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Mục lộ trình không tồn tại");
        }

        var path = await db.LearningPaths.FirstOrDefaultAsync(p => p.Id == node.PathId, ct);
        if (path is null || !CanManagePath(userId, role, path))
        {
            return Result.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền xóa mục này");
        }

        // Lấy tất cả node trong cùng path để truy vết toàn bộ cây con (descendant subtree)
        var allPathNodes = await db.LearningPathNodes.Where(n => n.PathId == node.PathId).ToListAsync(ct);
        var nodesToDelete = new List<LearningPathNode>();

        void CollectSubtree(int currentId)
        {
            var target = allPathNodes.FirstOrDefault(n => n.Id == currentId);
            if (target is not null && !nodesToDelete.Contains(target))
            {
                nodesToDelete.Add(target);
                var children = allPathNodes.Where(n => n.ParentId == currentId).ToList();
                foreach (var child in children)
                {
                    CollectSubtree(child.Id);
                }
            }
        }

        CollectSubtree(node.Id);

        var now = clock.UtcNow;
        var lessonIds = nodesToDelete.Where(n => n.LessonId != null).Select(n => n.LessonId!.Value).Distinct().ToList();
        if (lessonIds.Count > 0)
        {
            var lessons = await db.Lessons.Where(l => lessonIds.Contains(l.Id)).ToListAsync(ct);
            foreach (var l in lessons)
            {
                if (l.Status == LessonStatus.Hidden)
                {
                    l.DeletedAt = now;
                }
            }
        }

        var exerciseIds = nodesToDelete
            .SelectMany(n => new[] { n.FinalTestId, n.LabExerciseId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (exerciseIds.Count > 0)
        {
            var exercises = await db.Exercises.Where(e => exerciseIds.Contains(e.Id)).ToListAsync(ct);
            foreach (var e in exercises)
            {
                e.DeletedAt = now;
            }
        }

        // Soft delete (D6): chỉ đánh dấu DeletedAt thay vì RemoveRange — dòng node vẫn tồn tại để
        // UserNodeProgress / NodeSession / ClassAssignment (FK Restrict) không orphan và có thể khôi phục.
        // Global query filter (AppDbContext) tự ẩn các node này khỏi mọi truy vấn cây/bài học/báo cáo.
        foreach (var target in nodesToDelete)
        {
            target.DeletedAt = now;
        }
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Soft-deleted {Count} path items from Path {PathId} (Root node: {NodeId})",
            nodesToDelete.Count, node.PathId, node.Id);

        return Result.Ok();
    }

    public async Task<Result<PathItemDto>> FindByLessonIdAsync(int userId, string role, int lessonId, CancellationToken ct)
    {
        var node = await db.LearningPathNodes.AsNoTracking()
            .FirstOrDefaultAsync(n => n.LessonId == lessonId, ct);
        if (node is null)
        {
            return Result<PathItemDto>.Fail(ErrorCodes.NOT_FOUND, "Không tìm thấy mục lộ trình chứa bài học này");
        }

        var path = await db.LearningPaths.AsNoTracking().FirstOrDefaultAsync(p => p.Id == node.PathId, ct);
        if (path is null)
        {
            return Result<PathItemDto>.Fail(ErrorCodes.NOT_FOUND, "Lộ trình học không tồn tại");
        }

        if (!CanViewPath(userId, role, path))
        {
            return Result<PathItemDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền xem lộ trình này");
        }

        var dto = MapToDto(node);
        return Result<PathItemDto>.Ok(dto);
    }

    private static PathItemDto MapToDto(LearningPathNode node) => new()
    {
        Id = node.Id,
        PathId = node.PathId,
        ParentId = node.ParentId,
        ItemType = node.ItemType,
        Title = node.Title,
        Description = node.Description,
        LessonId = node.LessonId,
        FinalTestId = node.FinalTestId,
        LabExerciseId = node.LabExerciseId,
        SortOrder = node.SortOrder
    };

    private static List<PathItemDto> BuildHierarchy(List<PathItemDto> flatItems)
    {
        var itemMap = flatItems.ToDictionary(i => i.Id);
        var roots = new List<PathItemDto>();

        foreach (var item in flatItems.OrderBy(i => i.SortOrder))
        {
            if (item.ParentId is { } pId && itemMap.TryGetValue(pId, out var parent))
            {
                parent.Children.Add(item);
            }
            else
            {
                roots.Add(item);
            }
        }

        return roots;
    }

    private static List<string> DeserializeOptions(string optionsJson)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(optionsJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static List<int> DeserializeAnswer(string? answerJson)
    {
        if (string.IsNullOrWhiteSpace(answerJson)) return [0];
        try
        {
            return JsonSerializer.Deserialize<List<int>>(answerJson) ?? [0];
        }
        catch (JsonException)
        {
            return [0];
        }
    }

    /// <summary>
    /// Predicate phân quyền quản lý lộ trình (expose qua IPathItemService cho tầng API):
    /// ADMIN toàn quyền; TEACHER chỉ khi là chủ sở hữu path (CreatedBy hoặc AuthorId).
    /// </summary>
    public async Task<bool> CanManagePathAsync(int userId, string role, int pathId, CancellationToken ct)
    {
        var path = await db.LearningPaths.AsNoTracking().FirstOrDefaultAsync(p => p.Id == pathId, ct);
        return path is not null && CanManagePath(userId, role, path);
    }

    private static bool CanManagePath(int userId, string role, LearningPath path) =>
        role.Equals(RoleAdmin, StringComparison.OrdinalIgnoreCase)
        || path.CreatedBy == userId
        || path.AuthorId == userId;

    private static bool CanViewPath(int userId, string role, LearningPath path) =>
        CanManagePath(userId, role, path)
        || path.Status == LearningPathStatus.Active
        || path.Visibility == PathVisibility.Public;
}
