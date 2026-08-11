using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VisualizationDSA.Domain.Entities;
using VisualizationDSA.Domain.Enums;

namespace VisualizationDSA.Infrastructure.Data
{
    public partial class DbSeeder
    {
        // ══════════════════════════════════════════════════════════════════════
        // S7 — CLASSROOMS (2 lớp của teacher@, join code 6 ký tự, gắn roadmap
        // Sorting/Graph qua Course, 10 học sinh với tiến độ khác nhau).
        // Note: hệ thống dùng ClassroomEnrollment (không có bảng ClassroomMember).
        // ══════════════════════════════════════════════════════════════════════
        private async Task SeedClassroomsAsync()
        {
            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.Email == "teacher@visualizationdsa.dev");
            if (teacher == null) return;

            var sortingCourse = await _context.Courses.FirstOrDefaultAsync(c => c.Title == "Lộ trình Sắp xếp — Bài lý thuyết");
            var graphCourse = await _context.Courses.FirstOrDefaultAsync(c => c.Title == "Lộ trình Đồ thị — Bài lý thuyết");

            // Lớp 1 — gắn roadmap Sorting
            var class1 = await EnsureClassroomAsync(teacher.Id,
                "CNTT-K16 - Thuật toán",
                "Lớp học phần 'Thuật toán & Cấu trúc dữ liệu' — học kỳ 1. Chào mừng các bạn sinh viên K16!",
                "CNTT16", sortingCourse?.Id);

            // Lớp 2 — gắn roadmap Graph
            var class2 = await EnsureClassroomAsync(teacher.Id,
                "CNTT-K17 - Cấu trúc dữ liệu",
                "Lớp học phần 'Cấu trúc dữ liệu & Giải thuật' — học kỳ 2. Tập trung đồ thị và tối ưu hóa.",
                "CNTT17", graphCourse?.Id);

            if (class1 != null)
            {
                foreach (var email in new[] { "levanc@visualizationdsa.dev", "phamthid@visualizationdsa.dev", "hoangvane@visualizationdsa.dev", "vuthif@visualizationdsa.dev", "dangvang@visualizationdsa.dev", "buithih@visualizationdsa.dev" })
                {
                    await EnsureClassroomEnrollmentAsync(class1.Id, email);
                }
            }

            if (class2 != null)
            {
                foreach (var email in new[] { "nguyenvana@visualizationdsa.dev", "dovani@visualizationdsa.dev", "hongthikim@visualizationdsa.dev", "duongvanlam@visualizationdsa.dev" })
                {
                    await EnsureClassroomEnrollmentAsync(class2.Id, email);
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task<Classroom?> EnsureClassroomAsync(Guid teacherId, string name, string description, string inviteCode, Guid? courseId)
        {
            var classroom = await _context.Classrooms.FirstOrDefaultAsync(c => c.Name == name);
            if (classroom == null)
            {
                classroom = new Classroom(teacherId, name, description, inviteCode);
                _context.Classrooms.Add(classroom);
                await _context.SaveChangesAsync();
            }
            if (courseId.HasValue && classroom.CourseId != courseId.Value)
            {
                classroom.LinkToCourse(courseId.Value);
                await _context.SaveChangesAsync();
            }
            return classroom;
        }

        private async Task EnsureClassroomEnrollmentAsync(Guid classroomId, string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return;

            var exists = await _context.ClassroomEnrollments.AnyAsync(e => e.ClassroomId == classroomId && e.StudentId == user.Id);
            if (exists) return;

            _context.ClassroomEnrollments.Add(new ClassroomEnrollment(classroomId, user.Id));
        }
    }
}
