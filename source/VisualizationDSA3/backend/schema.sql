CREATE TABLE "AuditLogs" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AuditLogs" PRIMARY KEY,
    "Action" TEXT NOT NULL,
    "ActorId" TEXT NOT NULL,
    "ActorName" TEXT NOT NULL,
    "TargetId" TEXT NULL,
    "Details" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL
);


CREATE TABLE "Badges" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Badges" PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Icon" TEXT NOT NULL,
    "Color" TEXT NOT NULL,
    "Criteria" TEXT NOT NULL
);


CREATE TABLE "Codelabs" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Codelabs" PRIMARY KEY,
    "Title" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "InitialCode" TEXT NOT NULL,
    "Difficulty" INTEGER NOT NULL,
    "XPReward" INTEGER NOT NULL,
    "IsDeleted" INTEGER NOT NULL,
    "Constraints" TEXT NOT NULL,
    "Examples" TEXT NOT NULL,
    "Tags" TEXT NOT NULL,
    "MaxRuntimeMs" INTEGER NOT NULL,
    "MaxMemoryBytes" INTEGER NOT NULL,
    "AllowedLanguages" TEXT NOT NULL
);


CREATE TABLE "QuizXpGrants" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_QuizXpGrants" PRIMARY KEY,
    "UserId" TEXT NOT NULL,
    "QuizKey" TEXT NOT NULL,
    "GrantedAt" TEXT NOT NULL
);


CREATE TABLE "Quizzes" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Quizzes" PRIMARY KEY,
    "Title" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Topic" TEXT NOT NULL,
    "Difficulty" INTEGER NOT NULL DEFAULT 1,
    "XPReward" INTEGER NOT NULL,
    "IsDeleted" INTEGER NOT NULL
);


CREATE TABLE "SemanticConceptNodes" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_SemanticConceptNodes" PRIMARY KEY,
    "ConceptKey" TEXT NOT NULL,
    "Title" TEXT NOT NULL,
    "Category" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Embedding" TEXT NOT NULL,
    "Importance" REAL NOT NULL DEFAULT 0.0,
    "CreatedAt" TEXT NOT NULL
);


CREATE TABLE "SystemAuditEventStreams" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_SystemAuditEventStreams" PRIMARY KEY,
    "EventType" TEXT NOT NULL,
    "UserId" TEXT NULL,
    "CorrelationId" TEXT NULL,
    "HttpMethod" TEXT NULL,
    "Path" TEXT NULL,
    "StatusCode" INTEGER NULL,
    "Payload" TEXT NOT NULL,
    "Sequence" INTEGER NOT NULL,
    "OccurredAt" TEXT NOT NULL
);


CREATE TABLE "Users" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY,
    "Email" TEXT NOT NULL,
    "Username" TEXT NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "LastLoginAt" TEXT NULL,
    "TotalXP" INTEGER NOT NULL DEFAULT 0,
    "CurrentLevel" INTEGER NOT NULL DEFAULT 1,
    "StreakDays" INTEGER NOT NULL DEFAULT 0,
    "IsPremium" INTEGER NOT NULL,
    "Role" TEXT NOT NULL DEFAULT 'Student',
    "IsActive" INTEGER NOT NULL,
    "LastActivityDate" TEXT NULL
);


CREATE TABLE "CodelabHints" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_CodelabHints" PRIMARY KEY,
    "CodelabId" TEXT NOT NULL,
    "Content" TEXT NOT NULL,
    "IsTiered" INTEGER NOT NULL,
    "XpCost" INTEGER NOT NULL,
    "OrderIndex" INTEGER NOT NULL,
    CONSTRAINT "FK_CodelabHints_Codelabs_CodelabId" FOREIGN KEY ("CodelabId") REFERENCES "Codelabs" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "CodelabTemplates" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_CodelabTemplates" PRIMARY KEY,
    "CodelabId" TEXT NOT NULL,
    "Language" TEXT NOT NULL,
    "BoilerplateCode" TEXT NOT NULL,
    CONSTRAINT "FK_CodelabTemplates_Codelabs_CodelabId" FOREIGN KEY ("CodelabId") REFERENCES "Codelabs" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "CodelabTestCases" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_CodelabTestCases" PRIMARY KEY,
    "CodelabId" TEXT NOT NULL,
    "Input" TEXT NOT NULL,
    "ExpectedOutput" TEXT NOT NULL,
    "IsHidden" INTEGER NOT NULL,
    "ScoreWeight" INTEGER NOT NULL,
    "OrderIndex" INTEGER NOT NULL,
    CONSTRAINT "FK_CodelabTestCases_Codelabs_CodelabId" FOREIGN KEY ("CodelabId") REFERENCES "Codelabs" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "QuizQuestions" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_QuizQuestions" PRIMARY KEY,
    "QuizId" TEXT NOT NULL,
    "Question" TEXT NOT NULL,
    "Options" TEXT NOT NULL,
    "CorrectIndex" INTEGER NOT NULL,
    "Explanation" TEXT NOT NULL,
    CONSTRAINT "FK_QuizQuestions_Quizzes_QuizId" FOREIGN KEY ("QuizId") REFERENCES "Quizzes" ("Id") ON DELETE CASCADE
);


CREATE TABLE "KnowledgeEdges" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_KnowledgeEdges" PRIMARY KEY,
    "SourceNodeId" TEXT NOT NULL,
    "TargetNodeId" TEXT NOT NULL,
    "RelationType" TEXT NOT NULL,
    "Weight" REAL NOT NULL DEFAULT 1.0,
    "CreatedAt" TEXT NOT NULL,
    CONSTRAINT "FK_KnowledgeEdges_SemanticConceptNodes_SourceNodeId" FOREIGN KEY ("SourceNodeId") REFERENCES "SemanticConceptNodes" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_KnowledgeEdges_SemanticConceptNodes_TargetNodeId" FOREIGN KEY ("TargetNodeId") REFERENCES "SemanticConceptNodes" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Classrooms" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Classrooms" PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "OwnerTeacherId" TEXT NOT NULL,
    "CourseId" TEXT NULL,
    "ImportedFromCourseId" TEXT NULL,
    "InviteCode" TEXT NOT NULL,
    "IsArchived" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "InviteCodeExpiresAt" TEXT NULL,
    "MaxEnrollmentCapacity" INTEGER NULL,
    CONSTRAINT "FK_Classrooms_Users_OwnerTeacherId" FOREIGN KEY ("OwnerTeacherId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);


CREATE TABLE "CodelabSubmissions" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_CodelabSubmissions" PRIMARY KEY,
    "UserId" TEXT NOT NULL,
    "CodelabId" TEXT NOT NULL,
    "Code" TEXT NOT NULL,
    "Language" TEXT NOT NULL,
    "Status" TEXT NOT NULL,
    "ErrorMessage" TEXT NOT NULL,
    "RuntimeMs" INTEGER NOT NULL,
    "MemoryBytes" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "PassedCount" INTEGER NOT NULL,
    "TotalCount" INTEGER NOT NULL,
    "Score" INTEGER NOT NULL,
    "IsSubmit" INTEGER NOT NULL,
    "PerTestCaseResultJson" TEXT NOT NULL,
    CONSTRAINT "FK_CodelabSubmissions_Codelabs_CodelabId" FOREIGN KEY ("CodelabId") REFERENCES "Codelabs" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_CodelabSubmissions_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);


CREATE TABLE "Courses" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Courses" PRIMARY KEY,
    "TeacherId" TEXT NOT NULL,
    "Title" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "Category" INTEGER NOT NULL,
    "Difficulty" INTEGER NOT NULL,
    "IsPremium" INTEGER NOT NULL,
    "CoverImageUrl" TEXT NOT NULL,
    "IsPublished" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "IsDeleted" INTEGER NOT NULL,
    CONSTRAINT "FK_Courses_Users_TeacherId" FOREIGN KEY ("TeacherId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "LearningProgresses" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_LearningProgresses" PRIMARY KEY,
    "UserId" TEXT NOT NULL,
    "ModuleId" TEXT NOT NULL,
    "CompletedAt" TEXT NOT NULL,
    "TimeSpentMinutes" INTEGER NOT NULL,
    CONSTRAINT "FK_LearningProgresses_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);


CREATE TABLE "Lessons" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Lessons" PRIMARY KEY,
    "Title" TEXT NOT NULL,
    "ContentMd" TEXT NOT NULL,
    "SandboxType" TEXT NOT NULL,
    "SandboxConfig" TEXT NOT NULL,
    "XPReward" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "CreatedByTeacherId" TEXT NULL,
    "PublishStatus" INTEGER NOT NULL,
    "IsDeleted" INTEGER NOT NULL,
    CONSTRAINT "FK_Lessons_Users_CreatedByTeacherId" FOREIGN KEY ("CreatedByTeacherId") REFERENCES "Users" ("Id")
);


CREATE TABLE "Notifications" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Notifications" PRIMARY KEY,
    "UserId" TEXT NOT NULL,
    "Content" TEXT NOT NULL,
    "IsRead" INTEGER NOT NULL,
    "LinkUrl" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    CONSTRAINT "FK_Notifications_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);


CREATE TABLE "Orders" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_Orders" PRIMARY KEY,
    "UserId" TEXT NOT NULL,
    "PaymentCode" TEXT NOT NULL,
    "TransactionReference" TEXT NULL,
    "Amount" TEXT NOT NULL,
    "Status" TEXT NOT NULL DEFAULT 'Pending',
    "CreatedAt" TEXT NOT NULL,
    "CompletedAt" TEXT NULL,
    CONSTRAINT "FK_Orders_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);


CREATE TABLE "QuizAttempts" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_QuizAttempts" PRIMARY KEY,
    "UserId" TEXT NOT NULL,
    "QuizId" TEXT NOT NULL,
    "Score" INTEGER NOT NULL,
    "MaxScore" INTEGER NOT NULL,
    "Passed" INTEGER NOT NULL,
    "AttemptedAt" TEXT NOT NULL,
    "Answers" TEXT NOT NULL,
    CONSTRAINT "FK_QuizAttempts_Quizzes_QuizId" FOREIGN KEY ("QuizId") REFERENCES "Quizzes" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_QuizAttempts_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);


CREATE TABLE "RefreshTokens" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_RefreshTokens" PRIMARY KEY,
    "Token" TEXT NOT NULL,
    "UserId" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "ExpiresAt" TEXT NOT NULL,
    "IsRevoked" INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT "FK_RefreshTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);


CREATE TABLE "TheoryArticles" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_TheoryArticles" PRIMARY KEY,
    "AuthorId" TEXT NOT NULL,
    "Title" TEXT NOT NULL,
    "Slug" TEXT NOT NULL,
    "ContentMd" TEXT NOT NULL,
    "Category" TEXT NOT NULL,
    "Difficulty" TEXT NOT NULL,
    "Tags" TEXT NOT NULL,
    "ViewCount" INTEGER NOT NULL,
    "ReadTimeMinutes" INTEGER NOT NULL,
    "IsPublished" INTEGER NOT NULL,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "PublishedAt" TEXT NULL,
    "UpdatedAt" TEXT NOT NULL,
    CONSTRAINT "FK_TheoryArticles_Users_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "UserBadges" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_UserBadges" PRIMARY KEY,
    "UserId" TEXT NOT NULL,
    "BadgeId" TEXT NOT NULL,
    "EarnedAt" TEXT NOT NULL,
    CONSTRAINT "FK_UserBadges_Badges_BadgeId" FOREIGN KEY ("BadgeId") REFERENCES "Badges" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserBadges_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);


CREATE TABLE "CodelabHintReveals" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_CodelabHintReveals" PRIMARY KEY,
    "UserId" TEXT NOT NULL,
    "CodelabHintId" TEXT NOT NULL,
    "RevealedAt" TEXT NOT NULL,
    CONSTRAINT "FK_CodelabHintReveals_CodelabHints_CodelabHintId" FOREIGN KEY ("CodelabHintId") REFERENCES "CodelabHints" ("Id") ON DELETE CASCADE
);


CREATE TABLE "ClassroomAnnouncements" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_ClassroomAnnouncements" PRIMARY KEY,
    "ClassroomId" TEXT NOT NULL,
    "AuthorId" TEXT NOT NULL,
    "Title" TEXT NOT NULL,
    "ContentMd" TEXT NOT NULL,
    "IsPublished" INTEGER NOT NULL,
    "IsPinned" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "PublishedAt" TEXT NULL,
    "ClassroomId1" TEXT NULL,
    CONSTRAINT "FK_ClassroomAnnouncements_Classrooms_ClassroomId" FOREIGN KEY ("ClassroomId") REFERENCES "Classrooms" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ClassroomAnnouncements_Classrooms_ClassroomId1" FOREIGN KEY ("ClassroomId1") REFERENCES "Classrooms" ("Id"),
    CONSTRAINT "FK_ClassroomAnnouncements_Users_AuthorId" FOREIGN KEY ("AuthorId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "ClassroomEnrollments" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_ClassroomEnrollments" PRIMARY KEY,
    "ClassroomId" TEXT NOT NULL,
    "StudentId" TEXT NOT NULL,
    "JoinedAt" TEXT NOT NULL,
    "Status" INTEGER NOT NULL,
    "StatusChangedAt" TEXT NULL,
    "StatusChangedByUserId" TEXT NULL,
    "StatusChangeReason" TEXT NULL,
    CONSTRAINT "FK_ClassroomEnrollments_Classrooms_ClassroomId" FOREIGN KEY ("ClassroomId") REFERENCES "Classrooms" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ClassroomEnrollments_Users_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);


CREATE TABLE "ClassroomModules" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_ClassroomModules" PRIMARY KEY,
    "ClassroomId" TEXT NOT NULL,
    "Title" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "OrderIndex" INTEGER NOT NULL,
    "IsDeleted" INTEGER NOT NULL,
    "IsHidden" INTEGER NOT NULL,
    "UnlockAt" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    CONSTRAINT "FK_ClassroomModules_Classrooms_ClassroomId" FOREIGN KEY ("ClassroomId") REFERENCES "Classrooms" ("Id") ON DELETE CASCADE
);


CREATE TABLE "ClassroomQuizzes" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_ClassroomQuizzes" PRIMARY KEY,
    "ClassroomId" TEXT NOT NULL,
    "QuizId" TEXT NOT NULL,
    "OpenAt" TEXT NOT NULL,
    "DueAt" TEXT NOT NULL,
    "MaxAttempts" INTEGER NOT NULL,
    "IsArchived" INTEGER NOT NULL,
    CONSTRAINT "FK_ClassroomQuizzes_Classrooms_ClassroomId" FOREIGN KEY ("ClassroomId") REFERENCES "Classrooms" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ClassroomQuizzes_Quizzes_QuizId" FOREIGN KEY ("QuizId") REFERENCES "Quizzes" ("Id") ON DELETE CASCADE
);


CREATE TABLE "CourseModules" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_CourseModules" PRIMARY KEY,
    "CourseId" TEXT NOT NULL,
    "Title" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "OrderIndex" INTEGER NOT NULL,
    "IsDeleted" INTEGER NOT NULL,
    CONSTRAINT "FK_CourseModules_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE
);


CREATE TABLE "ClassroomLessons" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_ClassroomLessons" PRIMARY KEY,
    "ClassroomId" TEXT NOT NULL,
    "LessonId" TEXT NOT NULL,
    "OrderIndex" INTEGER NOT NULL,
    "UnlockAt" TEXT NULL,
    "IsVisible" INTEGER NOT NULL,
    CONSTRAINT "FK_ClassroomLessons_Classrooms_ClassroomId" FOREIGN KEY ("ClassroomId") REFERENCES "Classrooms" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ClassroomLessons_Lessons_LessonId" FOREIGN KEY ("LessonId") REFERENCES "Lessons" ("Id") ON DELETE CASCADE
);


CREATE TABLE "LessonComments" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_LessonComments" PRIMARY KEY,
    "LessonId" TEXT NOT NULL,
    "UserId" TEXT NOT NULL,
    "Content" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "ParentId" TEXT NULL,
    "IsEdited" INTEGER NOT NULL,
    "EditedAt" TEXT NULL,
    "IsDeleted" INTEGER NOT NULL,
    CONSTRAINT "FK_LessonComments_LessonComments_ParentId" FOREIGN KEY ("ParentId") REFERENCES "LessonComments" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_LessonComments_Lessons_LessonId" FOREIGN KEY ("LessonId") REFERENCES "Lessons" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_LessonComments_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);


CREATE TABLE "LessonReviews" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_LessonReviews" PRIMARY KEY,
    "LessonId" TEXT NOT NULL,
    "ReviewerAdminId" TEXT NULL,
    "IsApproved" INTEGER NULL,
    "Feedback" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    "ReviewedAt" TEXT NULL,
    CONSTRAINT "FK_LessonReviews_Lessons_LessonId" FOREIGN KEY ("LessonId") REFERENCES "Lessons" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_LessonReviews_Users_ReviewerAdminId" FOREIGN KEY ("ReviewerAdminId") REFERENCES "Users" ("Id")
);


CREATE TABLE "UserLessonProgresses" (
    "UserId" TEXT NOT NULL,
    "LessonId" TEXT NOT NULL,
    "Status" TEXT NOT NULL DEFAULT 'NotStarted',
    "CompletedAt" TEXT NULL,
    "XPRewarded" INTEGER NOT NULL,
    "LastActiveFrameIndex" INTEGER NOT NULL DEFAULT 0,
    "LastScrollPercent" REAL NOT NULL DEFAULT 0.0,
    "HasWatchedVisualizer" INTEGER NOT NULL,
    "QuizScore" INTEGER NULL,
    "BestScore" INTEGER NOT NULL,
    "CodelabCompleted" INTEGER NOT NULL,
    CONSTRAINT "PK_UserLessonProgresses" PRIMARY KEY ("UserId", "LessonId"),
    CONSTRAINT "FK_UserLessonProgresses_Lessons_LessonId" FOREIGN KEY ("LessonId") REFERENCES "Lessons" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserLessonProgresses_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);


CREATE TABLE "LessonTheoryArticles" (
    "LessonId" TEXT NOT NULL,
    "TheoryArticleId" TEXT NOT NULL,
    "OrderIndex" INTEGER NOT NULL,
    "AddedAt" TEXT NOT NULL,
    CONSTRAINT "PK_LessonTheoryArticles" PRIMARY KEY ("LessonId", "TheoryArticleId"),
    CONSTRAINT "FK_LessonTheoryArticles_Lessons_LessonId" FOREIGN KEY ("LessonId") REFERENCES "Lessons" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_LessonTheoryArticles_TheoryArticles_TheoryArticleId" FOREIGN KEY ("TheoryArticleId") REFERENCES "TheoryArticles" ("Id") ON DELETE CASCADE
);


CREATE TABLE "TheoryArticleVersions" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_TheoryArticleVersions" PRIMARY KEY,
    "ArticleId" TEXT NOT NULL,
    "ContentMd" TEXT NOT NULL,
    "ChangeSummary" TEXT NOT NULL,
    "ChangedBy" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "ChangedByUserId" TEXT NOT NULL,
    CONSTRAINT "FK_TheoryArticleVersions_TheoryArticles_ArticleId" FOREIGN KEY ("ArticleId") REFERENCES "TheoryArticles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_TheoryArticleVersions_Users_ChangedByUserId" FOREIGN KEY ("ChangedByUserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);


CREATE TABLE "ClassroomModuleItems" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_ClassroomModuleItems" PRIMARY KEY,
    "ModuleId" TEXT NOT NULL,
    "ItemType" INTEGER NOT NULL,
    "LessonId" TEXT NULL,
    "QuizId" TEXT NULL,
    "CodelabId" TEXT NULL,
    "OverrideTitle" TEXT NOT NULL,
    "OverrideDescription" TEXT NOT NULL,
    "OrderIndex" INTEGER NOT NULL,
    "IsRequired" INTEGER NOT NULL,
    "IsHidden" INTEGER NOT NULL,
    "IsDeleted" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "UnlockAt" TEXT NULL,
    "DueAt" TEXT NULL,
    "MaxAttempts" INTEGER NULL,
    "IsHiddenForStudent" INTEGER NOT NULL,
    "PrerequisiteItemId" TEXT NULL,
    "IsSequential" INTEGER NOT NULL,
    CONSTRAINT "CK_ClassroomModuleItem_OneReference" CHECK (("LessonId" IS NOT NULL AND "QuizId" IS NULL AND "CodelabId" IS NULL) OR ("LessonId" IS NULL AND "QuizId" IS NOT NULL AND "CodelabId" IS NULL) OR ("LessonId" IS NULL AND "QuizId" IS NULL AND "CodelabId" IS NOT NULL)),
    CONSTRAINT "FK_ClassroomModuleItems_ClassroomModules_ModuleId" FOREIGN KEY ("ModuleId") REFERENCES "ClassroomModules" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ClassroomModuleItems_Codelabs_CodelabId" FOREIGN KEY ("CodelabId") REFERENCES "Codelabs" ("Id"),
    CONSTRAINT "FK_ClassroomModuleItems_Lessons_LessonId" FOREIGN KEY ("LessonId") REFERENCES "Lessons" ("Id"),
    CONSTRAINT "FK_ClassroomModuleItems_Quizzes_QuizId" FOREIGN KEY ("QuizId") REFERENCES "Quizzes" ("Id")
);


CREATE TABLE "ClassroomQuizAttempts" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_ClassroomQuizAttempts" PRIMARY KEY,
    "ClassroomQuizId" TEXT NOT NULL,
    "StudentId" TEXT NOT NULL,
    "Score" INTEGER NOT NULL,
    "MaxScore" INTEGER NOT NULL,
    "SubmittedAt" TEXT NOT NULL,
    "IsLate" INTEGER NOT NULL,
    CONSTRAINT "FK_ClassroomQuizAttempts_ClassroomQuizzes_ClassroomQuizId" FOREIGN KEY ("ClassroomQuizId") REFERENCES "ClassroomQuizzes" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ClassroomQuizAttempts_Users_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);


CREATE TABLE "ModuleItems" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_ModuleItems" PRIMARY KEY,
    "ModuleId" TEXT NOT NULL,
    "ClassroomId" TEXT NULL,
    "ItemType" INTEGER NOT NULL,
    "LessonId" TEXT NULL,
    "QuizId" TEXT NULL,
    "CodelabId" TEXT NULL,
    "OverrideTitle" TEXT NOT NULL,
    "OrderIndex" INTEGER NOT NULL,
    "IsRequired" INTEGER NOT NULL,
    "IsDeleted" INTEGER NOT NULL,
    CONSTRAINT "CK_ModuleItem_OneReference" CHECK (("LessonId" IS NOT NULL AND "QuizId" IS NULL AND "CodelabId" IS NULL) OR ("LessonId" IS NULL AND "QuizId" IS NOT NULL AND "CodelabId" IS NULL) OR ("LessonId" IS NULL AND "QuizId" IS NULL AND "CodelabId" IS NOT NULL)),
    CONSTRAINT "FK_ModuleItems_Codelabs_CodelabId" FOREIGN KEY ("CodelabId") REFERENCES "Codelabs" ("Id"),
    CONSTRAINT "FK_ModuleItems_CourseModules_ModuleId" FOREIGN KEY ("ModuleId") REFERENCES "CourseModules" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ModuleItems_Lessons_LessonId" FOREIGN KEY ("LessonId") REFERENCES "Lessons" ("Id"),
    CONSTRAINT "FK_ModuleItems_Quizzes_QuizId" FOREIGN KEY ("QuizId") REFERENCES "Quizzes" ("Id")
);


CREATE TABLE "ClassroomModuleItemOverrides" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_ClassroomModuleItemOverrides" PRIMARY KEY,
    "ClassroomId" TEXT NOT NULL,
    "ModuleItemId" TEXT NOT NULL,
    "OpenAt" TEXT NULL,
    "DueAt" TEXT NULL,
    "MaxAttempts" INTEGER NULL,
    "IsHiddenForStudent" INTEGER NOT NULL,
    "PrerequisiteItemId" INTEGER NULL,
    "IsSequential" INTEGER NOT NULL,
    CONSTRAINT "FK_ClassroomModuleItemOverrides_Classrooms_ClassroomId" FOREIGN KEY ("ClassroomId") REFERENCES "Classrooms" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ClassroomModuleItemOverrides_ModuleItems_ModuleItemId" FOREIGN KEY ("ModuleItemId") REFERENCES "ModuleItems" ("Id") ON DELETE CASCADE
);


CREATE TABLE "UserModuleItemProgresses" (
    "UserId" TEXT NOT NULL,
    "ModuleItemId" TEXT NOT NULL,
    "AttemptNumber" INTEGER NOT NULL,
    "Status" TEXT NOT NULL DEFAULT 'NotStarted',
    "LastActiveFrameIndex" INTEGER NOT NULL DEFAULT 0,
    "LastScrollPercent" REAL NOT NULL DEFAULT 0.0,
    "ProgressPercent" REAL NOT NULL,
    "CompletedAt" TEXT NULL,
    "Score" INTEGER NULL,
    "LastAccessedAt" TEXT NOT NULL,
    CONSTRAINT "PK_UserModuleItemProgresses" PRIMARY KEY ("UserId", "ModuleItemId", "AttemptNumber"),
    CONSTRAINT "FK_UserModuleItemProgresses_ModuleItems_ModuleItemId" FOREIGN KEY ("ModuleItemId") REFERENCES "ModuleItems" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserModuleItemProgresses_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);


CREATE INDEX "IX_AuditLogs_CreatedAt" ON "AuditLogs" ("CreatedAt");


CREATE UNIQUE INDEX "IX_Badges_Name" ON "Badges" ("Name");


CREATE INDEX "IX_ClassroomAnnouncements_AuthorId" ON "ClassroomAnnouncements" ("AuthorId");


CREATE INDEX "IX_ClassroomAnnouncements_ClassroomId_PublishedAt" ON "ClassroomAnnouncements" ("ClassroomId", "PublishedAt");


CREATE INDEX "IX_ClassroomAnnouncements_ClassroomId1" ON "ClassroomAnnouncements" ("ClassroomId1");


CREATE UNIQUE INDEX "IX_ClassroomEnrollments_ClassroomId_StudentId" ON "ClassroomEnrollments" ("ClassroomId", "StudentId");


CREATE INDEX "IX_ClassroomEnrollments_StudentId" ON "ClassroomEnrollments" ("StudentId");


CREATE INDEX "IX_ClassroomLessons_ClassroomId" ON "ClassroomLessons" ("ClassroomId");


CREATE INDEX "IX_ClassroomLessons_LessonId" ON "ClassroomLessons" ("LessonId");


CREATE UNIQUE INDEX "IX_ClassroomModuleItemOverrides_ClassroomId_ModuleItemId" ON "ClassroomModuleItemOverrides" ("ClassroomId", "ModuleItemId");


CREATE INDEX "IX_ClassroomModuleItemOverrides_ModuleItemId" ON "ClassroomModuleItemOverrides" ("ModuleItemId");


CREATE INDEX "IX_ClassroomModuleItems_CodelabId" ON "ClassroomModuleItems" ("CodelabId");


CREATE INDEX "IX_ClassroomModuleItems_LessonId" ON "ClassroomModuleItems" ("LessonId");


CREATE UNIQUE INDEX "IX_ClassroomModuleItems_ModuleId_OrderIndex" ON "ClassroomModuleItems" ("ModuleId", "OrderIndex");


CREATE INDEX "IX_ClassroomModuleItems_QuizId" ON "ClassroomModuleItems" ("QuizId");


CREATE UNIQUE INDEX "IX_ClassroomModules_ClassroomId_OrderIndex" ON "ClassroomModules" ("ClassroomId", "OrderIndex");


CREATE INDEX "IX_ClassroomQuizAttempts_ClassroomQuizId" ON "ClassroomQuizAttempts" ("ClassroomQuizId");


CREATE INDEX "IX_ClassroomQuizAttempts_StudentId" ON "ClassroomQuizAttempts" ("StudentId");


CREATE INDEX "IX_ClassroomQuizzes_ClassroomId" ON "ClassroomQuizzes" ("ClassroomId");


CREATE INDEX "IX_ClassroomQuizzes_QuizId" ON "ClassroomQuizzes" ("QuizId");


CREATE INDEX "IX_Classrooms_OwnerTeacherId" ON "Classrooms" ("OwnerTeacherId");


CREATE INDEX "IX_CodelabHintReveals_CodelabHintId" ON "CodelabHintReveals" ("CodelabHintId");


CREATE UNIQUE INDEX "IX_CodelabHintReveals_UserId_CodelabHintId" ON "CodelabHintReveals" ("UserId", "CodelabHintId");


CREATE INDEX "IX_CodelabHints_CodelabId" ON "CodelabHints" ("CodelabId");


CREATE INDEX "IX_CodelabSubmissions_CodelabId" ON "CodelabSubmissions" ("CodelabId");


CREATE INDEX "IX_CodelabSubmissions_UserId_CodelabId_CreatedAt" ON "CodelabSubmissions" ("UserId", "CodelabId", "CreatedAt" DESC);


CREATE INDEX "IX_CodelabTemplates_CodelabId" ON "CodelabTemplates" ("CodelabId");


CREATE INDEX "IX_CodelabTestCases_CodelabId" ON "CodelabTestCases" ("CodelabId");


CREATE UNIQUE INDEX "IX_CourseModules_CourseId_OrderIndex" ON "CourseModules" ("CourseId", "OrderIndex");


CREATE INDEX "IX_Courses_TeacherId" ON "Courses" ("TeacherId");


CREATE INDEX "IX_KnowledgeEdges_RelationType" ON "KnowledgeEdges" ("RelationType");


CREATE UNIQUE INDEX "IX_KnowledgeEdges_SourceNodeId_TargetNodeId_RelationType" ON "KnowledgeEdges" ("SourceNodeId", "TargetNodeId", "RelationType");


CREATE INDEX "IX_KnowledgeEdges_TargetNodeId" ON "KnowledgeEdges" ("TargetNodeId");


CREATE UNIQUE INDEX "IX_LearningProgresses_UserId_ModuleId" ON "LearningProgresses" ("UserId", "ModuleId");


CREATE INDEX "IX_LessonComments_LessonId" ON "LessonComments" ("LessonId");


CREATE INDEX "IX_LessonComments_ParentId" ON "LessonComments" ("ParentId");


CREATE INDEX "IX_LessonComments_UserId" ON "LessonComments" ("UserId");


CREATE INDEX "IX_LessonReviews_LessonId" ON "LessonReviews" ("LessonId");


CREATE INDEX "IX_LessonReviews_ReviewerAdminId" ON "LessonReviews" ("ReviewerAdminId");


CREATE INDEX "IX_Lessons_CreatedByTeacherId" ON "Lessons" ("CreatedByTeacherId");


CREATE INDEX "IX_LessonTheoryArticles_TheoryArticleId" ON "LessonTheoryArticles" ("TheoryArticleId");


CREATE INDEX "IX_ModuleItems_CodelabId" ON "ModuleItems" ("CodelabId");


CREATE INDEX "IX_ModuleItems_LessonId" ON "ModuleItems" ("LessonId");


CREATE UNIQUE INDEX "IX_ModuleItems_ModuleId_OrderIndex" ON "ModuleItems" ("ModuleId", "OrderIndex");


CREATE INDEX "IX_ModuleItems_QuizId" ON "ModuleItems" ("QuizId");


CREATE INDEX "IX_Notifications_CreatedAt" ON "Notifications" ("CreatedAt");


CREATE INDEX "IX_Notifications_UserId" ON "Notifications" ("UserId");


CREATE UNIQUE INDEX "IX_Orders_PaymentCode" ON "Orders" ("PaymentCode");


CREATE UNIQUE INDEX "IX_Orders_TransactionReference" ON "Orders" ("TransactionReference");


CREATE INDEX "IX_Orders_UserId" ON "Orders" ("UserId");


CREATE INDEX "IX_QuizAttempts_QuizId" ON "QuizAttempts" ("QuizId");


CREATE INDEX "IX_QuizAttempts_UserId" ON "QuizAttempts" ("UserId");


CREATE INDEX "IX_QuizQuestions_QuizId" ON "QuizQuestions" ("QuizId");


CREATE UNIQUE INDEX "IX_RefreshTokens_Token" ON "RefreshTokens" ("Token");


CREATE INDEX "IX_RefreshTokens_UserId" ON "RefreshTokens" ("UserId");


CREATE INDEX "IX_SemanticConceptNodes_Category" ON "SemanticConceptNodes" ("Category");


CREATE UNIQUE INDEX "IX_SemanticConceptNodes_ConceptKey" ON "SemanticConceptNodes" ("ConceptKey");


CREATE INDEX "IX_SystemAuditEventStreams_EventType" ON "SystemAuditEventStreams" ("EventType");


CREATE INDEX "IX_SystemAuditEventStreams_OccurredAt" ON "SystemAuditEventStreams" ("OccurredAt");


CREATE INDEX "IX_SystemAuditEventStreams_Sequence" ON "SystemAuditEventStreams" ("Sequence");


CREATE INDEX "IX_SystemAuditEventStreams_UserId_OccurredAt" ON "SystemAuditEventStreams" ("UserId", "OccurredAt");


CREATE INDEX "IX_TheoryArticles_AuthorId" ON "TheoryArticles" ("AuthorId");


CREATE UNIQUE INDEX "IX_TheoryArticles_Slug" ON "TheoryArticles" ("Slug");


CREATE INDEX "IX_TheoryArticleVersions_ArticleId" ON "TheoryArticleVersions" ("ArticleId");


CREATE INDEX "IX_TheoryArticleVersions_ChangedByUserId" ON "TheoryArticleVersions" ("ChangedByUserId");


CREATE INDEX "IX_UserBadges_BadgeId" ON "UserBadges" ("BadgeId");


CREATE UNIQUE INDEX "IX_UserBadges_UserId_BadgeId" ON "UserBadges" ("UserId", "BadgeId");


CREATE INDEX "IX_UserLessonProgresses_LessonId" ON "UserLessonProgresses" ("LessonId");


CREATE INDEX "IX_UserModuleItemProgresses_ModuleItemId" ON "UserModuleItemProgresses" ("ModuleItemId");


CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");


CREATE INDEX "IX_Users_TotalXP" ON "Users" ("TotalXP");


CREATE UNIQUE INDEX "IX_Users_Username" ON "Users" ("Username");


