using System.Text.Json;
using System.Text.Json.Serialization;

namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Vai trò người dùng (SDD §7.3.1: 0=Student, 1=Teacher, 2=TeacherPending, 3=Admin).</summary>
public enum UserRole
{
    Student = 0,
    Teacher = 1,
    TeacherPending = 2,
    Admin = 3
}

/// <summary>Trạng thái bài học (SDD §7.3.2: 0=draft, 1=active, 2=hidden) — v2.15 mở rộng PendingReview.</summary>
[JsonConverter(typeof(LessonStatusJsonConverter))]
public enum LessonStatus
{
    Draft = 0,          // Bản nháp (chỉ người tạo thấy)
    PendingReview = 1,  // Chờ Admin duyệt để Public toàn sàn
    Active = 2,         // Đã xuất bản (sinh viên thấy)
    Hidden = 3          // Tạm ẩn
}

/// <summary>Trạng thái lộ trình học (0=Draft, 1=PendingReview, 2=Active, 3=Rejected).</summary>
[JsonConverter(typeof(LearningPathStatusJsonConverter))]
public enum LearningPathStatus
{
    Draft = 0,          // GV đang soạn
    PendingReview = 1,  // GV đã gửi duyệt, chờ Admin
    Active = 2,         // Admin đã duyệt, SV thấy được (Công khai)
    Rejected = 3,       // Admin từ chối, GV sửa lại
    ClassOnly = 4       // Dùng cho Lớp (GV tự gán lớp ngay, không cần duyệt admin)
}

public sealed class LessonStatusJsonConverter : JsonConverter<LessonStatus>
{
    public override LessonStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var intVal))
        {
            return Enum.IsDefined(typeof(LessonStatus), intVal) ? (LessonStatus)intVal : LessonStatus.Draft;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            if (string.IsNullOrWhiteSpace(str)) return LessonStatus.Draft;
            if (str.Equals("active", StringComparison.OrdinalIgnoreCase) || str.Equals("published", StringComparison.OrdinalIgnoreCase)) return LessonStatus.Active;
            if (str.Equals("pending_review", StringComparison.OrdinalIgnoreCase) || str.Equals("pendingreview", StringComparison.OrdinalIgnoreCase)) return LessonStatus.PendingReview;
            if (str.Equals("hidden", StringComparison.OrdinalIgnoreCase)) return LessonStatus.Hidden;
            if (str.Equals("draft", StringComparison.OrdinalIgnoreCase)) return LessonStatus.Draft;
            if (Enum.TryParse<LessonStatus>(str, true, out var parsed)) return parsed;
        }

        return LessonStatus.Draft;
    }

    public override void Write(Utf8JsonWriter writer, LessonStatus value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString().ToLowerInvariant());
    }
}

public sealed class LearningPathStatusJsonConverter : JsonConverter<LearningPathStatus>
{
    public override LearningPathStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var intVal))
        {
            return Enum.IsDefined(typeof(LearningPathStatus), intVal) ? (LearningPathStatus)intVal : LearningPathStatus.Draft;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            if (string.IsNullOrWhiteSpace(str)) return LearningPathStatus.Draft;
            if (str.Equals("active", StringComparison.OrdinalIgnoreCase) || str.Equals("published", StringComparison.OrdinalIgnoreCase) || str.Equals("public", StringComparison.OrdinalIgnoreCase)) return LearningPathStatus.Active;
            if (str.Equals("pending_review", StringComparison.OrdinalIgnoreCase) || str.Equals("pendingreview", StringComparison.OrdinalIgnoreCase)) return LearningPathStatus.PendingReview;
            if (str.Equals("class", StringComparison.OrdinalIgnoreCase) || str.Equals("classonly", StringComparison.OrdinalIgnoreCase) || str.Equals("class_only", StringComparison.OrdinalIgnoreCase)) return LearningPathStatus.ClassOnly;
            if (str.Equals("rejected", StringComparison.OrdinalIgnoreCase)) return LearningPathStatus.Rejected;
            if (str.Equals("draft", StringComparison.OrdinalIgnoreCase)) return LearningPathStatus.Draft;
            if (Enum.TryParse<LearningPathStatus>(str, true, out var parsed)) return parsed;
        }

        return LearningPathStatus.Draft;
    }

    public override void Write(Utf8JsonWriter writer, LearningPathStatus value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString().ToLowerInvariant());
    }
}

/// <summary>Khả kiến lộ trình học (0=Private: chỉ tác giả/admin, 1=ClassOnly: tác giả/admin/lớp trỏ vào, 2=Public: hiển thị toàn sàn khi Active).</summary>
[JsonConverter(typeof(PathVisibilityJsonConverter))]
public enum PathVisibility
{
    Private = 0,
    ClassOnly = 1,
    Public = 2
}

public sealed class PathVisibilityJsonConverter : JsonConverter<PathVisibility>
{
    public override PathVisibility Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var intVal))
        {
            return Enum.IsDefined(typeof(PathVisibility), intVal) ? (PathVisibility)intVal : PathVisibility.Private;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            if (string.IsNullOrWhiteSpace(str)) return PathVisibility.Private;
            if (str.Equals("public", StringComparison.OrdinalIgnoreCase)) return PathVisibility.Public;
            if (str.Equals("class", StringComparison.OrdinalIgnoreCase) || str.Equals("classonly", StringComparison.OrdinalIgnoreCase) || str.Equals("class_only", StringComparison.OrdinalIgnoreCase)) return PathVisibility.ClassOnly;
            if (str.Equals("private", StringComparison.OrdinalIgnoreCase) || str.Equals("draft", StringComparison.OrdinalIgnoreCase)) return PathVisibility.Private;
            if (Enum.TryParse<PathVisibility>(str, true, out var parsed)) return parsed;
        }

        return PathVisibility.Private;
    }

    public override void Write(Utf8JsonWriter writer, PathVisibility value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString().ToLowerInvariant());
    }
}

/// <summary>Loại node trong cây lộ trình (0=Folder, 1=Theory, 2=Quiz, 3=Lab).</summary>
[JsonConverter(typeof(PathItemTypeJsonConverter))]
public enum PathItemType
{
    Folder = 0,   // thư mục/module — KHÔNG có nội dung, chỉ chứa con
    Theory = 1,   // trỏ LessonId
    Quiz = 2,     // trỏ FinalTestId (exercise type Mcq)
    Lab = 3       // trỏ LabExerciseId (exercise type Code hoặc SimulationLab)
}

public sealed class PathItemTypeJsonConverter : JsonConverter<PathItemType>
{
    public override PathItemType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var intVal))
        {
            return Enum.IsDefined(typeof(PathItemType), intVal) ? (PathItemType)intVal : PathItemType.Theory;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            if (string.IsNullOrWhiteSpace(str)) return PathItemType.Theory;
            if (str.Equals("folder", StringComparison.OrdinalIgnoreCase) || str.Equals("module", StringComparison.OrdinalIgnoreCase)) return PathItemType.Folder;
            if (str.Equals("theory", StringComparison.OrdinalIgnoreCase) || str.Equals("lesson", StringComparison.OrdinalIgnoreCase)) return PathItemType.Theory;
            if (str.Equals("quiz", StringComparison.OrdinalIgnoreCase) || str.Equals("mcq", StringComparison.OrdinalIgnoreCase)) return PathItemType.Quiz;
            if (str.Equals("lab", StringComparison.OrdinalIgnoreCase) || str.Equals("code", StringComparison.OrdinalIgnoreCase) || str.Equals("codelab", StringComparison.OrdinalIgnoreCase)) return PathItemType.Lab;
            if (Enum.TryParse<PathItemType>(str, true, out var parsed)) return parsed;
        }

        return PathItemType.Theory;
    }

    public override void Write(Utf8JsonWriter writer, PathItemType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString().ToLowerInvariant());
    }
}

/// <summary>Loại bài tập (SDD §7.3.9: 0=MCQ, 1=SIMULATION_PREDICT, 2=SIMULATION_LAB, 3=CODE).</summary>
public enum ExerciseType
{
    Mcq = 0,
    SimulationPredict = 1,
    SimulationLab = 2,
    Code = 3
}

/// <summary>Trạng thái bài tập (SDD §7.3.9: 0=draft, 1=active).</summary>
public enum ExerciseStatus
{
    Draft = 0,
    Active = 1
}

/// <summary>Loại câu hỏi (API_REFERENCE.md §3.8/§4.6: SINGLE/MULTI/BOOLEAN/LAB).</summary>
public enum QuestionType
{
    Single = 0,
    Multi = 1,
    Boolean = 2,
    Lab = 3
}

/// <summary>Trạng thái lớp (SDD §7.3.16: 0=Mở, 1=Đóng).</summary>
public enum ClassStatus
{
    Open = 0,
    Closed = 1
}

/// <summary>Trạng thái bug report (SDD §7.3.22: 0=mới, 1=đang xử lý, 2=đã xử lý, 3=đóng).</summary>
public enum BugReportStatus
{
    New = 0,
    Processing = 1,
    Resolved = 2,
    Closed = 3
}

/// <summary>Trạng thái CodeRun (SDD §7.3.23: 0 chờ, 1 chạy, 2 thành công, 3 lỗi, 4 timeout).</summary>
public enum CodeRunStatus
{
    Pending = 0,
    Running = 1,
    Success = 2,
    Error = 3,
    Timeout = 4
}

/// <summary>Loại ý kiến học viên gửi giảng viên về khóa học.</summary>
public enum CourseFeedbackType
{
    Suggestion = 0,   // Góp ý
    Bug = 1,          // Báo lỗi
    Request = 2       // Đề xuất nội dung
}

/// <summary>Trạng thái xử lý ý kiến học viên (pattern BugReport — SDD §7.3.22).</summary>
public enum CourseFeedbackStatus
{
    New = 0,       // Mới — chưa ai đọc
    Read = 1,      // Đã đọc
    Resolved = 2   // Đã xử lý / đã trả lời
}
