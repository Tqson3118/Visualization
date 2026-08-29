# Phase B Real Data Seed Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans.

**Goal:** Seed deterministic realistic data for 20 courses, 128 users, 12 classes, and required gamification/payment/feedback coverage without breaking API contracts.

**Architecture:** Extend the existing DbSeeder orchestration with focused idempotent methods. Reuse existing domain constructors and relationships; do not reconstruct the absent PR30 seed-data tree or add schema changes. Generate roster identities from fixed arrays and stable email keys, then seed dependent rows from resolved entities.

**Tech Stack:** .NET 10, EF Core 10, SQLite, existing VisualizationDSA domain entities and ApplicationDbContext.

## Global Constraints
- Stop WebApi before deleting SQLite or building because DLL/database may be locked.
- Run seed only through Program.cs startup; never run a separate seed process while WebApi serves.
- Do not use git add -A; stage only Phase B backend, docs-sync, tracking, and report/account paths.
- Preserve existing API shapes and compatible existing seed content.
- Password/account file is local-only and must not be committed.
- Phase B requires two clean database rebuilds with matching roster/counts.
- Unrelated WIP test compile failures are outside this phase; use backend build and database/query gates.

## Tasks

### Task 1: Deterministic seeder primitives
Modify DbSeeder.cs. Add fixed seed password/clock, roster arrays, course definitions, transliteration, level calculation, metadata serialization, stable lookup/creation, and a SeedRealDataAsync orchestration called from SeedAsync. Write credentials only to ignored future/real-data-accounts.md. Verify compilation before dependent work.

### Task 2: Roster and account state
Create 8 Teacher, 1 Admin, 120 Student, and 8 PendingTeacher users. Match anchor emails/names/XP/streak/premium values from prompt. Generate ranks 25-120 deterministically, ensure unique emails, assign 12 class buckets, and initialize new users with 10/10 hearts. Verify counts, duplicates, and login credentials.

### Task 3: Courses and learning content
Create or extend 20 published courses with teacher authors, metadata JSON, at least 3 lessons and 2 quizzes/exercises per course, retaining richer existing content for courses 1-10. Verify course/lesson/quiz counts and API list responses.

### Task 4: Classes, progress, and feedback
Create 12 SE1801-SE1812 classes, 10 student memberships each, realistic assignments/curriculum, Bảo progress, CourseReview, LessonComment, LessonReview, learning-path, notes, and favorites rows only where constructors/schema support them. Verify foreign keys and review eligibility.

### Task 5: Gamification, shop, and payments
Create 14 badges and assign all to Bảo, daily/weekly/monthly quests with mixed Bảo states, 10-12 shop items, equipped frame/avatar inventory, non-negative gem transaction balances, and 25-40 orders across 14 days with valid payment codes and recent completed revenue. Verify all related endpoints.

### Task 6: Reproducibility gate
Stop WebApi, delete the SQLite file, start on port 5055, inspect startup log, query deterministic counts, then repeat the delete/start cycle and compare snapshots. Save evidence to future/real-data-seed-gate.md and update the handoff.

### Task 7: Review and checkpoint
Run git diff --check, fresh backend build, code review for correctness/idempotency/security/performance, update tracking where encoding permits, and create checkpoint commit 2 with only Phase B paths.

## Risks and mitigations
| Risk | Mitigation |
|---|---|
| Monolithic seeder growth | Add bounded helpers and explicit orchestration; no broad refactor. |
| Missing constructors/entities | Inspect every entity before use; seed only supported tables. |
| SQLite lock | Stop WebApi before DB deletion/build; start only for verification. |
| Duplicate users | Stable email lookup plus unique/count gates. |
| Existing seed collisions | Preserve records and use natural-key upserts. |
| Time-box overrun | Prioritize roster/gamification and record reduced scope instead of faking data. |
