# Real-Data Phase B Consistency Verification

**Date:** 2026-08-19  
**Status:** 100% PASS (All SQL consistency queries satisfied)

## Database Integrity & Consistency Checks (§2.5)

### 1. Leaderboard Top Anchors
Top 24 students sorted by `TotalXp DESC` match the deterministic anchor roster:
- **Rank 1:** Lê Quốc Bảo (`baolqse1801@fpt.edu.vn`, SE1801) — 3800 XP, Streak 21, Level 10, Premium: TRUE
- **Rank 2:** Trần Thị Hồng Nhung (`nhungtthse1802@fpt.edu.vn`, SE1802) — 3550 XP, Streak 28, Level 9, Premium: TRUE
- **Rank 3:** Phạm Minh Đức (`ducpmse1803@fpt.edu.vn`, SE1803) — 3200 XP, Streak 14, Level 9
- **Rank 4:** Nguyễn Hoàng Anh (`anhnhse1804@fpt.edu.vn`, SE1804) — 2900 XP, Streak 19, Level 8
- **Rank 5:** Vũ Thị Mai Linh (`linhvtmse1805@fpt.edu.vn`, SE1805) — 2650 XP, Streak 15, Level 8, Premium: TRUE
- Remaining anchors 6–24 follow strict descending XP down to rank 24 (230 XP).

### 2. CourseReviews & Testimonials
- Total reviews: 80 reviews across all 20 courses (4 reviews / course).
- Average rating: 4.5 / 5.0.
- All reviewers are students from the roster.

### 3. Gem Ledger Balance
- `SELECT UserId, SUM(CASE WHEN Type='Earn' THEN Amount ELSE -Amount END) AS balance FROM GemTransactions GROUP BY UserId HAVING SUM(...) < 0;`
- **Result:** 0 rows (No user has negative gem balance).

### 4. Premium Orders vs IsPremium Flag
- `SELECT o.Id FROM Orders o JOIN Users u ON u.Id = o.UserId WHERE o.Status = 'Completed' AND o.Amount >= 199000 AND u.IsPremium = 0;`
- **Result:** 0 rows (All completed orders >= 199,000đ belong strictly to users with `IsPremium = true`).

### 5. Email Uniqueness & Role Validity
- Duplicate emails: 0 rows.
- Invalid roles: 0 rows (All 137 users belong to `'Student'`, `'Teacher'`, `'PendingTeacher'`, or `'Admin'`).

## Scale & Coverage Counts Summary

| Entity | Count | Details |
|---|---|---|
| **Users** | 137 | 120 Students + 8 Teachers + 8 Pending Teachers + 1 Admin |
| **Courses** | 20 | 10 PR30 based + 10 advanced courses, all 25 simulator keys covered |
| **Course Modules** | 60 | 3 modules / course |
| **Lessons** | 100 | 5 lessons / course with real Markdown content |
| **Quizzes** | 52 | Comprehensive coverage across all courses |
| **Quiz Questions** | 418 | Multiple choice questions with full explanations |
| **Classrooms** | 12 | SE1801 – SE1812, 10 students / class (120 total enrollments) |
| **Classroom Enrollments** | 120 | 100% student enrollment coverage |
| **Orders** | 30 | Spread over 14 days; realistic 7-day revenue chart |
| **Badges** | 22 | 22 badge definitions; 14 earned by anchor Lê Quốc Bảo |
| **Quests** | 7 | 4 Daily, 2 Weekly, 1 Monthly; progress active for Bảo |
| **Shop Items** | 10 | Avatars, frames, themes, misc |
| **User Inventory** | 2 | Equipped frame & avatar for Bảo |
| **Codelabs** | 6 | Real code templates, test cases, and hints |
| **Theory Articles** | 8 | Real theory content linked to lessons |
| **Learning Paths** | 3 | Full knowledge progression nodes |
