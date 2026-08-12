namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Vai trò người dùng (SDD §7.3.1: 0=Student, 1=Teacher, 2=TeacherPending, 3=Admin).</summary>
public enum UserRole
{
    Student = 0,
    Teacher = 1,
    TeacherPending = 2,
    Admin = 3
}

/// <summary>Trạng thái bài học (SDD §7.3.2: 0=draft, 1=active, 2=hidden).</summary>
public enum LessonStatus
{
    Draft = 0,
    Active = 1,
    Hidden = 2
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
