import re

with open(r'd:\FPT\og\VisualizationDSA\backend\src\Infrastructure\Data\DbSeeder.cs', 'r', encoding='utf-8') as f:
    content = f.read()

new_seed_courses = """        private async Task SeedCoursesAsync()
        {
            if (await _context.Courses.CountAsync() > 0)
            {
                return;
            }

            _context.ModuleItems.RemoveRange(_context.ModuleItems);
            _context.CourseModules.RemoveRange(_context.CourseModules);
            _context.Lessons.RemoveRange(_context.Lessons);
            _context.Courses.RemoveRange(_context.Courses);
            _context.CodelabTestCases.RemoveRange(_context.CodelabTestCases);
            _context.Codelabs.RemoveRange(_context.Codelabs);
            _context.Quizzes.RemoveRange(_context.Quizzes);
            await _context.SaveChangesAsync();

            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.Role == "Teacher" || u.Role == "Admin")
                          ?? await _context.Users.FirstOrDefaultAsync();
            if (teacher == null)
            {
                teacher = new User("teacher@visualizationdsa.dev", "Default Teacher", HashPasswordSHA256("Teacher@2024"));
                teacher.SetRole("Teacher");
                await _context.Users.AddAsync(teacher);
                await _context.SaveChangesAsync();
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string seedContentPath = Path.Combine(baseDir, "Data", "SeedContent");
            if (!Directory.Exists(seedContentPath)) 
            {
                seedContentPath = Path.Combine(baseDir, "..", "..", "..", "Infrastructure", "Data", "SeedContent");
            }

            string coursesJsonPath = Path.Combine(seedContentPath, "courses.json");
            if (!File.Exists(coursesJsonPath))
            {
                Console.WriteLine($"[SeedCourses] Seed file not found at {coursesJsonPath}");
                return;
            }

            string jsonContent = await File.ReadAllTextAsync(coursesJsonPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var seedCourses = JsonSerializer.Deserialize<List<SeedCourseModel>>(jsonContent, options);

            if (seedCourses == null) return;

            foreach (var sc in seedCourses)
            {
                var diff = Enum.TryParse<CourseDifficulty>(sc.Difficulty, out var parsedDiff) ? parsedDiff : CourseDifficulty.Beginner;
                var cat = Enum.TryParse<CourseCategory>(sc.Category, out var parsedCat) ? parsedCat : CourseCategory.DataStructure;
                
                var course = new Course(teacher.Id, sc.Title, sc.Description, cat, diff, true, sc.ImageUrl);
                await _context.Courses.AddAsync(course);
                await _context.SaveChangesAsync(); 

                var module = new CourseModule(course.Id, "Chương 1", "Nội dung chính", 1000);
                course.Modules.Add(module);
                await _context.SaveChangesAsync();

                int itemOrder = 1000;
                foreach (var sl in sc.Lessons)
                {
                    string markdownContent = "";
                    string mdPath = Path.Combine(seedContentPath, sl.ContentFile);
                    if (File.Exists(mdPath)) markdownContent = await File.ReadAllTextAsync(mdPath);

                    var lesson = new Lesson(sl.Title, markdownContent, sl.SandboxType, sl.SandboxConfig, sl.XpReward, teacher.Id);
                    await _context.Lessons.AddAsync(lesson);
                    await _context.SaveChangesAsync();

                    var lessonItem = new ModuleItem(module.Id, null, VisualizationDSA.Domain.Enums.ModuleItemType.Lesson, lesson.Id, null, null, sl.Title, itemOrder++, true);
                    module.Items.Add(lessonItem);

                    if (sl.Quiz != null)
                    {
                        var quiz = new Quiz(sl.Quiz.Title, "Trắc nghiệm ôn tập", "general", sl.Quiz.Questions.Count, sl.XpReward + 10);
                        foreach (var sq in sl.Quiz.Questions)
                        {
                            quiz.AddQuestion(sq.Content, sq.Options.Select(o => o.Text).ToArray(), sq.Options.FindIndex(o => o.IsCorrect), sq.Options.FirstOrDefault(o => o.IsCorrect)?.Explanation ?? "");
                        }
                        await _context.Quizzes.AddAsync(quiz);
                        await _context.SaveChangesAsync();

                        var quizItem = new ModuleItem(module.Id, null, VisualizationDSA.Domain.Enums.ModuleItemType.Quiz, null, quiz.Id, null, "Quiz: " + sl.Title, itemOrder++, true);
                        module.Items.Add(quizItem);
                    }

                    if (sl.Codelab != null)
                    {
                        var codelab = new Codelab(
                            sl.Codelab.Title, sl.Codelab.Description, sl.Codelab.InitialCode,
                            1, 50, 5000, 128000000, "csharp", "Vui lòng xem mô tả", "Xem ví dụ trong mô tả", "", "general"
                        );
                        
                        int caseIndex = 1;
                        foreach (var tc in sl.Codelab.TestCases)
                        {
                            codelab.TestCases.Add(new CodelabTestCase(codelab.Id, tc.Input, tc.ExpectedOutput, tc.IsHidden, 10, caseIndex++));
                        }
                        await _context.Codelabs.AddAsync(codelab);
                        await _context.SaveChangesAsync();

                        var codelabItem = new ModuleItem(module.Id, null, VisualizationDSA.Domain.Enums.ModuleItemType.Codelab, null, null, codelab.Id, "Codelab: " + sl.Title, itemOrder++, true);
                        module.Items.Add(codelabItem);
                    }
                }
            }
        }

        private class SeedCourseModel
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string Difficulty { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public List<SeedLessonModel> Lessons { get; set; } = new();
        }

        private class SeedLessonModel
        {
            public string Title { get; set; } = string.Empty;
            public string ContentFile { get; set; } = string.Empty;
            public string SandboxType { get; set; } = string.Empty;
            public string SandboxConfig { get; set; } = string.Empty;
            public int XpReward { get; set; }
            public SeedQuizModel? Quiz { get; set; }
            public SeedCodelabModel? Codelab { get; set; }
        }

        private class SeedQuizModel
        {
            public string Title { get; set; } = string.Empty;
            public int PassingScore { get; set; }
            public List<SeedQuestionModel> Questions { get; set; } = new();
        }

        private class SeedQuestionModel
        {
            public string Content { get; set; } = string.Empty;
            public List<SeedOptionModel> Options { get; set; } = new();
        }

        private class SeedOptionModel
        {
            public string Text { get; set; } = string.Empty;
            public bool IsCorrect { get; set; }
            public string Explanation { get; set; } = string.Empty;
        }

        private class SeedCodelabModel
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string InitialCode { get; set; } = string.Empty;
            public List<SeedTestCaseModel> TestCases { get; set; } = new();
        }

        private class SeedTestCaseModel
        {
            public string Input { get; set; } = string.Empty;
            public string ExpectedOutput { get; set; } = string.Empty;
            public bool IsHidden { get; set; }
        }
    }
}"""

start_idx = content.find("        private async Task SeedCoursesAsync()")
if start_idx != -1:
    content = content[:start_idx] + new_seed_courses
    with open(r'd:\FPT\og\VisualizationDSA\backend\src\Infrastructure\Data\DbSeeder.cs', 'w', encoding='utf-8') as f:
        f.write(content)
    print("Patched successfully")
else:
    print("Could not find SeedCoursesAsync")
