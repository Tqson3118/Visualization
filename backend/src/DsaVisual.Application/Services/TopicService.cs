using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Services;

/// <summary>
/// TopicService thật theo SDD §7.3.7 / API_REFERENCE.md §4.3.
/// Tên trùng cùng cấp cha → VALIDATION_FAILED (field name); xóa chặn khi có bài học → TOPIC_HAS_LESSONS;
/// reorder dùng body <c>{ ids: [...] }</c> (SortOrder = vị trí trong mảng).
/// </summary>
public sealed class TopicService(
    AppDbContext db,
    IDateTimeProvider clock,
    ILogger<TopicService> logger) : ITopicService
{
    public async Task<Result<List<TopicDto>>> GetTreeAsync(CancellationToken ct)
    {
        var topics = await db.Topics.AsNoTracking()
            .Where(t => t.DeletedAt == null && !t.Name.StartsWith("Module "))
            .OrderBy(t => t.SortOrder).ThenBy(t => t.Id)
            .ToListAsync(ct);

        var nodes = topics.ToDictionary(t => t.Id, ToDto);
        var roots = new List<TopicDto>();

        foreach (var topic in topics)
        {
            if (topic.ParentId is { } parentId && nodes.TryGetValue(parentId, out var parent))
            {
                parent.Children.Add(nodes[topic.Id]);
            }
            else
            {
                roots.Add(nodes[topic.Id]);
            }
        }

        return Result<List<TopicDto>>.Ok(roots);
    }

    public async Task<Result<TopicDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var topic = await db.Topics.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null, ct);
        return topic is null
            ? Result<TopicDto>.Fail(ErrorCodes.NOT_FOUND, "Chủ đề không tồn tại")
            : Result<TopicDto>.Ok(ToDto(topic));
    }

    public async Task<Result<TopicDto>> CreateAsync(int userId, TopicUpsertRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<TopicDto>.Fail(ErrorCodes.VALIDATION_FAILED, "Tên chủ đề không được để trống", new()
            {
                ["name"] = ["Tên chủ đề không được để trống"]
            });
        }

        var name = request.Name.Trim();
        if (request.ParentId is { } parentId)
        {
            var parent = await db.Topics.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == parentId && t.DeletedAt == null, ct);
            if (parent is null)
            {
                return Result<TopicDto>.Fail(ErrorCodes.NOT_FOUND, "Chủ đề cha không tồn tại");
            }

            if (parent.ParentId != null)
            {
                // Cây tối đa 2 cấp (SDD §7.3.7)
                return Result<TopicDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                    "Chủ đề chỉ được lồng tối đa 2 cấp", new() { ["parentId"] = ["Chủ đề chỉ được lồng tối đa 2 cấp"] });
            }

            if (await SiblingNameExistsAsync(null, parentId, name, ct))
            {
                return Result<TopicDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                    "Tên chủ đề đã tồn tại", new() { ["name"] = ["Tên chủ đề đã tồn tại"] });
            }
        }
        else if (await SiblingNameExistsAsync(null, null, name, ct))
        {
            return Result<TopicDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Tên chủ đề đã tồn tại", new() { ["name"] = ["Tên chủ đề đã tồn tại"] });
        }

        var topic = new Topic
        {
            ParentId = request.ParentId,
            Name = name,
            Description = request.Description?.Trim(),
            SortOrder = request.SortOrder,
            CreatedBy = userId,
            CreatedAt = clock.UtcNow
        };

        db.Topics.Add(topic);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Topic {TopicId} created by user {UserId}", topic.Id, userId);
        return Result<TopicDto>.Ok(ToDto(topic));
    }

    public async Task<Result<TopicDto>> UpdateAsync(int userId, string role, int id, TopicUpsertRequest request, CancellationToken ct)
    {
        var topic = await db.Topics.FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null, ct);
        if (topic is null)
        {
            return Result<TopicDto>.Fail(ErrorCodes.NOT_FOUND, "Chủ đề không tồn tại");
        }

        // v2.15 (Vấn đề 3): Teacher chỉ sửa được Topic do chính mình tạo; Admin sửa được tất cả
        if (!IsAdmin(role) && topic.CreatedBy != userId)
        {
            return Result<TopicDto>.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền chỉnh sửa hoặc xóa chủ đề này");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<TopicDto>.Fail(ErrorCodes.VALIDATION_FAILED, "Tên chủ đề không được để trống", new()
            {
                ["name"] = ["Tên chủ đề không được để trống"]
            });
        }

        var name = request.Name.Trim();
        if (await SiblingNameExistsAsync(id, request.ParentId, name, ct))
        {
            return Result<TopicDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Tên chủ đề đã tồn tại", new() { ["name"] = ["Tên chủ đề đã tồn tại"] });
        }

        if (request.ParentId is { } parentId && parentId != id)
        {
            var parent = await db.Topics.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == parentId && t.DeletedAt == null, ct);
            if (parent is null)
            {
                return Result<TopicDto>.Fail(ErrorCodes.NOT_FOUND, "Chủ đề cha không tồn tại");
            }

            if (parent.ParentId != null)
            {
                return Result<TopicDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                    "Chủ đề chỉ được lồng tối đa 2 cấp", new() { ["parentId"] = ["Chủ đề chỉ được lồng tối đa 2 cấp"] });
            }
        }

        topic.ParentId = request.ParentId;
        topic.Name = name;
        topic.Description = request.Description?.Trim();
        topic.SortOrder = request.SortOrder;
        topic.UpdatedAt = clock.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Topic {TopicId} updated", id);
        return Result<TopicDto>.Ok(ToDto(topic));
    }

    public async Task<Result> DeleteAsync(int userId, string role, int id, CancellationToken ct)
    {
        var topic = await db.Topics.FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null, ct);
        if (topic is null)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Chủ đề không tồn tại");
        }

        // v2.15 (Vấn đề 3): Teacher chỉ xóa được Topic do chính mình tạo; Admin xóa được tất cả
        if (!IsAdmin(role) && topic.CreatedBy != userId)
        {
            return Result.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền chỉnh sửa hoặc xóa chủ đề này");
        }

        var hasLessons = await db.Lessons.AsNoTracking().AnyAsync(l => l.TopicId == id && l.DeletedAt == null, ct);
        if (hasLessons)
        {
            return Result.Fail(ErrorCodes.TOPIC_HAS_LESSONS, "Không xóa được chủ đề có bài học");
        }

        var hasChildren = await db.Topics.AsNoTracking().AnyAsync(t => t.ParentId == id && t.DeletedAt == null, ct);
        if (hasChildren)
        {
            return Result.Fail(ErrorCodes.TOPIC_HAS_LESSONS, "Không xóa được chủ đề có chủ đề con");
        }

        topic.DeletedAt = clock.UtcNow;
        topic.UpdatedAt = clock.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Topic {TopicId} soft-deleted", id);
        return Result.Ok();
    }

    public async Task<Result> ReorderAsync(TopicReorderRequest request, CancellationToken ct)
    {
        if (request.Ids.Count == 0)
        {
            return Result.Fail(ErrorCodes.VALIDATION_FAILED, "Danh sách chủ đề rỗng", new()
            {
                ["ids"] = ["Danh sách chủ đề rỗng"]
            });
        }

        var topics = await db.Topics
            .Where(t => request.Ids.Contains(t.Id) && t.DeletedAt == null)
            .ToListAsync(ct);

        if (topics.Count != request.Ids.Distinct().Count())
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Một số chủ đề không tồn tại");
        }

        for (var i = 0; i < request.Ids.Count; i++)
        {
            var topic = topics.First(t => t.Id == request.Ids[i]);
            topic.SortOrder = i;
            topic.UpdatedAt = clock.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Topics reordered: {Ids}", string.Join(',', request.Ids));
        return Result.Ok();
    }

    private const string RoleAdmin = "ADMIN";

    private static bool IsAdmin(string role) =>
        role.Equals(RoleAdmin, StringComparison.OrdinalIgnoreCase);

    private async Task<bool> SiblingNameExistsAsync(int? excludeId, int? parentId, string name, CancellationToken ct)
    {
        // So sánh không phân biệt hoa thường để nhất quán mọi provider (InMemory ordinal vs SQL Server collation)
        var siblings = await db.Topics.AsNoTracking()
            .Where(t => t.DeletedAt == null && t.ParentId == parentId)
            .Select(t => new { t.Id, t.Name })
            .ToListAsync(ct);

        return siblings.Any(t => t.Id != excludeId && t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private static TopicDto ToDto(Topic topic) => new()
    {
        Id = topic.Id,
        ParentId = topic.ParentId,
        Name = topic.Name,
        Description = topic.Description,
        SortOrder = topic.SortOrder,
        CreatedBy = topic.CreatedBy
    };
}
