using DsaVisual.Application.Persistence.Entities;

namespace DsaVisual.Application.Common;

/// <summary>
/// Ánh xạ UserRole ↔ chuỗi role chuẩn API (API_REFERENCE.md §3.3: STUDENT/TEACHER/TEACHER_PENDING/ADMIN).
/// </summary>
public static class RoleNames
{
    public const string Student = "STUDENT";
    public const string Teacher = "TEACHER";
    public const string TeacherPending = "TEACHER_PENDING";
    public const string Admin = "ADMIN";

    public static string ToApi(UserRole role) => role switch
    {
        UserRole.Teacher => Teacher,
        UserRole.TeacherPending => TeacherPending,
        UserRole.Admin => Admin,
        _ => Student
    };

    public static bool TryParse(string? value, out UserRole role)
    {
        switch (value?.Trim().ToUpperInvariant())
        {
            case Student:
                role = UserRole.Student;
                return true;
            case Teacher:
                role = UserRole.Teacher;
                return true;
            case TeacherPending:
                role = UserRole.TeacherPending;
                return true;
            case Admin:
                role = UserRole.Admin;
                return true;
            default:
                role = UserRole.Student;
                return false;
        }
    }
}
