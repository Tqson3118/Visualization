using System.Text.Json;
using System.Text.Json.Serialization;
using DsaVisual.Application.Persistence.Entities;

namespace DsaVisual.UnitTests;

public class EnumSerializationTests
{
    private readonly JsonSerializerOptions _options;

    public EnumSerializationTests()
    {
        _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        _options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));
    }

    [Theory]
    [InlineData(LessonStatus.Draft, "\"draft\"")]
    [InlineData(LessonStatus.PendingReview, "\"pendingreview\"")]
    [InlineData(LessonStatus.Active, "\"active\"")]
    [InlineData(LessonStatus.Hidden, "\"hidden\"")]
    public void LessonStatus_SerializesAndDeserializesCorrectly(LessonStatus status, string json)
    {
        var deserialized = JsonSerializer.Deserialize<LessonStatus>(json, _options);
        Assert.Equal(status, deserialized);
    }

    [Theory]
    [InlineData(LearningPathStatus.Draft, "\"draft\"")]
    [InlineData(LearningPathStatus.PendingReview, "\"pendingreview\"")]
    [InlineData(LearningPathStatus.Active, "\"active\"")]
    [InlineData(LearningPathStatus.Rejected, "\"rejected\"")]
    public void LearningPathStatus_SerializesAndDeserializesCorrectly(LearningPathStatus status, string json)
    {
        var deserialized = JsonSerializer.Deserialize<LearningPathStatus>(json, _options);
        Assert.Equal(status, deserialized);
    }

    [Theory]
    [InlineData(UserRole.Student, "\"student\"")]
    [InlineData(UserRole.Teacher, "\"teacher\"")]
    [InlineData(UserRole.TeacherPending, "\"teacherPending\"")]
    [InlineData(UserRole.Admin, "\"admin\"")]
    public void UserRole_SerializesAndDeserializesCorrectly(UserRole role, string json)
    {
        var serialized = JsonSerializer.Serialize(role, _options);
        var deserialized = JsonSerializer.Deserialize<UserRole>(json, _options);
        Assert.Equal(role, deserialized);
    }

    [Theory]
    [InlineData(ExerciseType.Mcq, "\"mcq\"")]
    [InlineData(ExerciseType.SimulationPredict, "\"simulationPredict\"")]
    [InlineData(ExerciseType.SimulationLab, "\"simulationLab\"")]
    [InlineData(ExerciseType.Code, "\"code\"")]
    public void ExerciseType_SerializesAndDeserializesCorrectly(ExerciseType type, string json)
    {
        var serialized = JsonSerializer.Serialize(type, _options);
        var deserialized = JsonSerializer.Deserialize<ExerciseType>(json, _options);
        Assert.Equal(type, deserialized);
    }

    [Theory]
    [InlineData(BugReportStatus.New, "\"new\"")]
    [InlineData(BugReportStatus.Processing, "\"processing\"")]
    [InlineData(BugReportStatus.Resolved, "\"resolved\"")]
    [InlineData(BugReportStatus.Closed, "\"closed\"")]
    public void BugReportStatus_SerializesAndDeserializesCorrectly(BugReportStatus status, string json)
    {
        var serialized = JsonSerializer.Serialize(status, _options);
        var deserialized = JsonSerializer.Deserialize<BugReportStatus>(json, _options);
        Assert.Equal(status, deserialized);
    }
}
