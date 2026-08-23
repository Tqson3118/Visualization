IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Achievements] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(100) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IconUrl] nvarchar(500) NULL,
    [ConditionJson] nvarchar(max) NOT NULL,
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_Achievements] PRIMARY KEY ([Id])
);

CREATE TABLE [DailyQuests] (
    [Id] int NOT NULL IDENTITY,
    [QuestKey] nvarchar(100) NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Type] int NOT NULL,
    [ConditionJson] nvarchar(max) NOT NULL,
    [RewardJson] nvarchar(max) NOT NULL,
    [PoolEnabled] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK_DailyQuests] PRIMARY KEY ([Id])
);

CREATE TABLE [ShopItems] (
    [Id] int NOT NULL IDENTITY,
    [ItemKey] nvarchar(100) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [PriceGems] int NOT NULL DEFAULT 0,
    [MaxStack] int NOT NULL DEFAULT 1,
    [Type] int NOT NULL,
    [DurationHours] int NULL,
    CONSTRAINT [PK_ShopItems] PRIMARY KEY ([Id])
);

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [Email] nvarchar(256) NOT NULL,
    [PasswordHash] nvarchar(256) NOT NULL,
    [DisplayName] nvarchar(100) NOT NULL,
    [Role] int NOT NULL DEFAULT 0,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [IsPrimaryAdmin] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [AvatarUrl] nvarchar(500) NULL,
    [Hearts] int NOT NULL DEFAULT 10,
    [HeartsMax] int NOT NULL DEFAULT 10,
    [LastHeartAt] datetime2 NOT NULL,
    [Gems] int NOT NULL,
    [Xp] int NOT NULL,
    [StreakDays] int NOT NULL,
    [StreakFreeze] int NOT NULL,
    [PremiumUntil] datetime2 NULL,
    [LastActivityDate] datetime2 NULL,
    [StreakLastProcessed] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);

CREATE TABLE [BugReports] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NULL,
    [Description] nvarchar(2000) NOT NULL,
    [ContextJson] nvarchar(max) NULL,
    [Status] int NOT NULL DEFAULT 0,
    [AssigneeId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ResolvedAt] datetime2 NULL,
    CONSTRAINT [PK_BugReports] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BugReports_Users_AssigneeId] FOREIGN KEY ([AssigneeId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_BugReports_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Classes] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [InviteCode] nvarchar(6) NOT NULL,
    [Semester] nvarchar(50) NULL,
    [Description] nvarchar(500) NULL,
    [OwnerId] int NOT NULL,
    [Status] int NOT NULL DEFAULT 0,
    [CreatedAt] datetime2 NOT NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_Classes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Classes_Users_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Favorites] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [SimulationKey] nvarchar(100) NOT NULL,
    [InputJson] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Favorites] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Favorites_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [GemTransactions] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Type] int NOT NULL,
    [Amount] int NOT NULL,
    [RefType] nvarchar(50) NULL,
    [RefId] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_GemTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GemTransactions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PasswordResetTokens] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [TokenHash] nvarchar(64) NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [Used] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_PasswordResetTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PasswordResetTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PremiumSubscriptions] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [PlanId] nvarchar(50) NULL,
    [StartedAt] datetime2 NOT NULL,
    [ExpiresAt] datetime2 NULL,
    [Status] int NOT NULL DEFAULT 0,
    [OrderRef] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_PremiumSubscriptions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PremiumSubscriptions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [RefreshTokens] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [TokenHash] nvarchar(64) NOT NULL,
    [PreviousTokenHash] nvarchar(64) NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [RevokedAt] datetime2 NULL,
    [CreatedByIp] nvarchar(45) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Settings] (
    [Id] int NOT NULL IDENTITY,
    [Key] nvarchar(100) NOT NULL,
    [Value] nvarchar(500) NOT NULL,
    [Description] nvarchar(500) NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [UpdatedBy] int NOT NULL,
    CONSTRAINT [PK_Settings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Settings_Users_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Topics] (
    [Id] int NOT NULL IDENTITY,
    [ParentId] int NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [SortOrder] int NOT NULL DEFAULT 0,
    [CreatedBy] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_Topics] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Topics_Topics_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Topics] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Topics_Users_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [UserAchievements] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [AchievementId] int NOT NULL,
    [EarnedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_UserAchievements] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserAchievements_Achievements_AchievementId] FOREIGN KEY ([AchievementId]) REFERENCES [Achievements] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserAchievements_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [UserInventory] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [ItemId] int NOT NULL,
    [Quantity] int NOT NULL DEFAULT 1,
    [IsEquipped] bit NOT NULL DEFAULT CAST(0 AS bit),
    [PurchasedAt] datetime2 NOT NULL,
    [ExpiresAt] datetime2 NULL,
    CONSTRAINT [PK_UserInventory] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserInventory_ShopItems_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [ShopItems] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserInventory_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [UserQuests] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [QuestId] int NOT NULL,
    [QuestDate] datetime2 NOT NULL,
    [Progress] int NOT NULL DEFAULT 0,
    [Claimed] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_UserQuests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserQuests_DailyQuests_QuestId] FOREIGN KEY ([QuestId]) REFERENCES [DailyQuests] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserQuests_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ClassMembers] (
    [Id] int NOT NULL IDENTITY,
    [ClassId] int NOT NULL,
    [UserId] int NOT NULL,
    [JoinedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ClassMembers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ClassMembers_Classes_ClassId] FOREIGN KEY ([ClassId]) REFERENCES [Classes] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ClassMembers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [LearningPaths] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(500) NULL,
    [TopicId] int NULL,
    [SortOrder] int NOT NULL DEFAULT 0,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedBy] int NOT NULL,
    CONSTRAINT [PK_LearningPaths] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LearningPaths_Topics_TopicId] FOREIGN KEY ([TopicId]) REFERENCES [Topics] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_LearningPaths_Users_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Lessons] (
    [Id] int NOT NULL IDENTITY,
    [TopicId] int NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(500) NULL,
    [ContentHtml] nvarchar(max) NOT NULL,
    [SortOrder] int NOT NULL DEFAULT 0,
    [Status] int NOT NULL DEFAULT 0,
    [CreatedBy] int NOT NULL,
    [UpdatedBy] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_Lessons] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Lessons_Topics_TopicId] FOREIGN KEY ([TopicId]) REFERENCES [Topics] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Lessons_Users_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Lessons_Users_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ContentFeedback] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [LessonId] int NOT NULL,
    [Rating] int NOT NULL,
    [Comment] nvarchar(200) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_ContentFeedback] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContentFeedback_Lessons_LessonId] FOREIGN KEY ([LessonId]) REFERENCES [Lessons] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ContentFeedback_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [LessonNotes] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [LessonId] int NOT NULL,
    [ContentHtml] nvarchar(max) NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_LessonNotes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LessonNotes_Lessons_LessonId] FOREIGN KEY ([LessonId]) REFERENCES [Lessons] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_LessonNotes_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [LessonSimulations] (
    [Id] int NOT NULL IDENTITY,
    [LessonId] int NOT NULL,
    [SimulationKey] nvarchar(max) NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [DefaultInputJson] nvarchar(max) NULL,
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_LessonSimulations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LessonSimulations_Lessons_LessonId] FOREIGN KEY ([LessonId]) REFERENCES [Lessons] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [UserProgress] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [LessonId] int NOT NULL,
    [Viewed] bit NOT NULL,
    [SimulationCount] int NOT NULL,
    [BestScore] int NULL,
    [CompletedAt] datetime2 NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_UserProgress] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserProgress_Lessons_LessonId] FOREIGN KEY ([LessonId]) REFERENCES [Lessons] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserProgress_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ClassAssignments] (
    [Id] int NOT NULL IDENTITY,
    [ClassId] int NOT NULL,
    [LessonId] int NULL,
    [ExerciseId] int NULL,
    [DueAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ClassAssignments] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_ClassAssignments_Content] CHECK (([LessonId] IS NOT NULL OR [ExerciseId] IS NOT NULL)),
    CONSTRAINT [FK_ClassAssignments_Classes_ClassId] FOREIGN KEY ([ClassId]) REFERENCES [Classes] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ClassAssignments_Lessons_LessonId] FOREIGN KEY ([LessonId]) REFERENCES [Lessons] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [CodeRuns] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [ExerciseId] int NULL,
    [Code] nvarchar(max) NOT NULL,
    [InputJson] nvarchar(max) NOT NULL,
    [Status] int NOT NULL DEFAULT 0,
    [OutputJson] nvarchar(max) NULL,
    [ErrorJson] nvarchar(max) NULL,
    [TraceJson] nvarchar(max) NULL,
    [DurationMs] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_CodeRuns] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CodeRuns_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [CodeSubmissions] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [ExerciseId] int NOT NULL,
    [Code] nvarchar(max) NOT NULL,
    [Score] int NOT NULL,
    [PassedTests] int NOT NULL,
    [TotalTests] int NOT NULL,
    [ResultJson] nvarchar(max) NOT NULL,
    [SubmittedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_CodeSubmissions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CodeSubmissions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Exercises] (
    [Id] int NOT NULL IDENTITY,
    [LessonId] int NOT NULL,
    [NodeId] int NULL,
    [Stage] int NULL,
    [ConfigJson] nvarchar(max) NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(500) NULL,
    [Type] int NOT NULL DEFAULT 0,
    [DurationMinutes] int NULL,
    [MaxScore] int NOT NULL,
    [Status] int NOT NULL DEFAULT 0,
    [CreatedBy] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [DeletedAt] datetime2 NULL,
    CONSTRAINT [PK_Exercises] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Exercises_Lessons_LessonId] FOREIGN KEY ([LessonId]) REFERENCES [Lessons] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Exercises_Users_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ExerciseSubmissions] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [ExerciseId] int NOT NULL,
    [ClassAssignmentId] int NULL,
    [Score] int NOT NULL,
    [AnswersJson] nvarchar(max) NOT NULL,
    [ResultJson] nvarchar(max) NOT NULL,
    [DurationSeconds] int NULL,
    [SubmittedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ExerciseSubmissions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ExerciseSubmissions_ClassAssignments_ClassAssignmentId] FOREIGN KEY ([ClassAssignmentId]) REFERENCES [ClassAssignments] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExerciseSubmissions_Exercises_ExerciseId] FOREIGN KEY ([ExerciseId]) REFERENCES [Exercises] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ExerciseSubmissions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [LearningPathNodes] (
    [Id] int NOT NULL IDENTITY,
    [PathId] int NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [LessonId] int NULL,
    [SortOrder] int NOT NULL,
    [FinalTestId] int NULL,
    CONSTRAINT [PK_LearningPathNodes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LearningPathNodes_Exercises_FinalTestId] FOREIGN KEY ([FinalTestId]) REFERENCES [Exercises] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_LearningPathNodes_LearningPaths_PathId] FOREIGN KEY ([PathId]) REFERENCES [LearningPaths] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LearningPathNodes_Lessons_LessonId] FOREIGN KEY ([LessonId]) REFERENCES [Lessons] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Questions] (
    [Id] int NOT NULL IDENTITY,
    [ExerciseId] int NOT NULL,
    [Type] int NOT NULL DEFAULT 0,
    [Content] nvarchar(max) NOT NULL,
    [OptionsJson] nvarchar(max) NOT NULL,
    [AnswerJson] nvarchar(max) NOT NULL,
    [Explanation] nvarchar(max) NULL,
    [Hint1] nvarchar(500) NULL,
    [Hint2] nvarchar(500) NULL,
    [Hint3] nvarchar(500) NULL,
    [WrongExplanationsJson] nvarchar(max) NULL,
    [KeepOrder] bit NOT NULL,
    [Points] int NOT NULL DEFAULT 1,
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_Questions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Questions_Exercises_ExerciseId] FOREIGN KEY ([ExerciseId]) REFERENCES [Exercises] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [NodeSessions] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [NodeId] int NOT NULL,
    [StartedAt] datetime2 NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [Stage] int NULL,
    [StepIndex] int NULL,
    CONSTRAINT [PK_NodeSessions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_NodeSessions_LearningPathNodes_NodeId] FOREIGN KEY ([NodeId]) REFERENCES [LearningPathNodes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_NodeSessions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [UserNodeProgress] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [NodeId] int NOT NULL,
    [Status] int NOT NULL,
    [Stars] int NOT NULL,
    [NodeScore] int NOT NULL,
    [UnlockedAt] datetime2 NULL,
    [PassedAt] datetime2 NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_UserNodeProgress] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserNodeProgress_LearningPathNodes_NodeId] FOREIGN KEY ([NodeId]) REFERENCES [LearningPathNodes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserNodeProgress_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX [IX_Achievements_Code] ON [Achievements] ([Code]);

CREATE INDEX [IX_BugReports_AssigneeId] ON [BugReports] ([AssigneeId]);

CREATE INDEX [IX_BugReports_Status_CreatedAt] ON [BugReports] ([Status], [CreatedAt]);

CREATE INDEX [IX_BugReports_UserId] ON [BugReports] ([UserId]);

CREATE INDEX [IX_ClassAssignments_ClassId_DueAt] ON [ClassAssignments] ([ClassId], [DueAt]);

CREATE INDEX [IX_ClassAssignments_ExerciseId] ON [ClassAssignments] ([ExerciseId]);

CREATE INDEX [IX_ClassAssignments_LessonId] ON [ClassAssignments] ([LessonId]);

CREATE UNIQUE INDEX [IX_Classes_InviteCode] ON [Classes] ([InviteCode]);

CREATE INDEX [IX_Classes_OwnerId] ON [Classes] ([OwnerId]);

CREATE UNIQUE INDEX [IX_ClassMembers_ClassId_UserId] ON [ClassMembers] ([ClassId], [UserId]);

CREATE INDEX [IX_ClassMembers_UserId] ON [ClassMembers] ([UserId]);

CREATE INDEX [IX_CodeRuns_ExerciseId_Status] ON [CodeRuns] ([ExerciseId], [Status]);

CREATE INDEX [IX_CodeRuns_UserId_CreatedAt] ON [CodeRuns] ([UserId], [CreatedAt]);

CREATE INDEX [IX_CodeSubmissions_ExerciseId] ON [CodeSubmissions] ([ExerciseId]);

CREATE INDEX [IX_CodeSubmissions_UserId_ExerciseId_SubmittedAt] ON [CodeSubmissions] ([UserId], [ExerciseId], [SubmittedAt]);

CREATE INDEX [IX_ContentFeedback_LessonId] ON [ContentFeedback] ([LessonId]);

CREATE UNIQUE INDEX [IX_ContentFeedback_UserId_LessonId] ON [ContentFeedback] ([UserId], [LessonId]);

CREATE UNIQUE INDEX [IX_DailyQuests_QuestKey] ON [DailyQuests] ([QuestKey]);

CREATE INDEX [IX_Exercises_CreatedBy] ON [Exercises] ([CreatedBy]);

CREATE INDEX [IX_Exercises_LessonId] ON [Exercises] ([LessonId]);

CREATE INDEX [IX_Exercises_NodeId_Stage] ON [Exercises] ([NodeId], [Stage]);

CREATE INDEX [IX_ExerciseSubmissions_ClassAssignmentId] ON [ExerciseSubmissions] ([ClassAssignmentId]);

CREATE INDEX [IX_ExerciseSubmissions_ExerciseId] ON [ExerciseSubmissions] ([ExerciseId]);

CREATE INDEX [IX_ExerciseSubmissions_UserId_ExerciseId_SubmittedAt] ON [ExerciseSubmissions] ([UserId], [ExerciseId], [SubmittedAt]);

CREATE UNIQUE INDEX [IX_Favorites_UserId_SimulationKey] ON [Favorites] ([UserId], [SimulationKey]);

CREATE INDEX [IX_GemTransactions_UserId_CreatedAt] ON [GemTransactions] ([UserId], [CreatedAt]);

CREATE INDEX [IX_LearningPathNodes_FinalTestId] ON [LearningPathNodes] ([FinalTestId]);

CREATE INDEX [IX_LearningPathNodes_LessonId] ON [LearningPathNodes] ([LessonId]);

CREATE UNIQUE INDEX [IX_LearningPathNodes_PathId_SortOrder] ON [LearningPathNodes] ([PathId], [SortOrder]);

CREATE INDEX [IX_LearningPaths_CreatedBy] ON [LearningPaths] ([CreatedBy]);

CREATE INDEX [IX_LearningPaths_TopicId] ON [LearningPaths] ([TopicId]);

CREATE INDEX [IX_LessonNotes_LessonId] ON [LessonNotes] ([LessonId]);

CREATE UNIQUE INDEX [IX_LessonNotes_UserId_LessonId] ON [LessonNotes] ([UserId], [LessonId]);

CREATE INDEX [IX_Lessons_CreatedBy_Status] ON [Lessons] ([CreatedBy], [Status]);

CREATE INDEX [IX_Lessons_TopicId] ON [Lessons] ([TopicId]);

CREATE INDEX [IX_Lessons_UpdatedBy] ON [Lessons] ([UpdatedBy]);

CREATE INDEX [IX_LessonSimulations_LessonId] ON [LessonSimulations] ([LessonId]);

CREATE INDEX [IX_NodeSessions_ExpiresAt] ON [NodeSessions] ([ExpiresAt]);

CREATE INDEX [IX_NodeSessions_NodeId] ON [NodeSessions] ([NodeId]);

CREATE UNIQUE INDEX [IX_NodeSessions_UserId_NodeId] ON [NodeSessions] ([UserId], [NodeId]);

CREATE UNIQUE INDEX [IX_PasswordResetTokens_TokenHash] ON [PasswordResetTokens] ([TokenHash]);

CREATE INDEX [IX_PasswordResetTokens_UserId] ON [PasswordResetTokens] ([UserId]);

CREATE INDEX [IX_PremiumSubscriptions_Status_ExpiresAt] ON [PremiumSubscriptions] ([Status], [ExpiresAt]);

CREATE INDEX [IX_PremiumSubscriptions_UserId_Status] ON [PremiumSubscriptions] ([UserId], [Status]);

CREATE INDEX [IX_Questions_ExerciseId] ON [Questions] ([ExerciseId]);

CREATE UNIQUE INDEX [IX_RefreshTokens_TokenHash] ON [RefreshTokens] ([TokenHash]);

CREATE INDEX [IX_RefreshTokens_UserId_ExpiresAt] ON [RefreshTokens] ([UserId], [ExpiresAt]);

CREATE UNIQUE INDEX [IX_Settings_Key] ON [Settings] ([Key]);

CREATE INDEX [IX_Settings_UpdatedBy] ON [Settings] ([UpdatedBy]);

CREATE UNIQUE INDEX [IX_ShopItems_ItemKey] ON [ShopItems] ([ItemKey]);

CREATE INDEX [IX_Topics_CreatedBy] ON [Topics] ([CreatedBy]);

CREATE UNIQUE INDEX [IX_Topics_Name] ON [Topics] ([Name]) WHERE [ParentId] IS NULL;

CREATE UNIQUE INDEX [IX_Topics_ParentId_Name] ON [Topics] ([ParentId], [Name]) WHERE [ParentId] IS NOT NULL;

CREATE INDEX [IX_UserAchievements_AchievementId] ON [UserAchievements] ([AchievementId]);

CREATE INDEX [IX_UserAchievements_UserId] ON [UserAchievements] ([UserId]);

CREATE UNIQUE INDEX [IX_UserAchievements_UserId_AchievementId] ON [UserAchievements] ([UserId], [AchievementId]);

CREATE INDEX [IX_UserInventory_ItemId] ON [UserInventory] ([ItemId]);

CREATE UNIQUE INDEX [IX_UserInventory_UserId_ItemId] ON [UserInventory] ([UserId], [ItemId]);

CREATE INDEX [IX_UserNodeProgress_NodeId] ON [UserNodeProgress] ([NodeId]);

CREATE UNIQUE INDEX [IX_UserNodeProgress_UserId_NodeId] ON [UserNodeProgress] ([UserId], [NodeId]);

CREATE INDEX [IX_UserProgress_LessonId] ON [UserProgress] ([LessonId]);

CREATE UNIQUE INDEX [IX_UserProgress_UserId_LessonId] ON [UserProgress] ([UserId], [LessonId]);

CREATE INDEX [IX_UserQuests_QuestId] ON [UserQuests] ([QuestId]);

CREATE UNIQUE INDEX [IX_UserQuests_UserId_QuestDate_QuestId] ON [UserQuests] ([UserId], [QuestDate], [QuestId]);

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

CREATE INDEX [IX_Users_PremiumUntil] ON [Users] ([PremiumUntil]);

CREATE INDEX [IX_Users_Role_IsActive] ON [Users] ([Role], [IsActive]);

ALTER TABLE [ClassAssignments] ADD CONSTRAINT [FK_ClassAssignments_Exercises_ExerciseId] FOREIGN KEY ([ExerciseId]) REFERENCES [Exercises] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [CodeRuns] ADD CONSTRAINT [FK_CodeRuns_Exercises_ExerciseId] FOREIGN KEY ([ExerciseId]) REFERENCES [Exercises] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [CodeSubmissions] ADD CONSTRAINT [FK_CodeSubmissions_Exercises_ExerciseId] FOREIGN KEY ([ExerciseId]) REFERENCES [Exercises] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [Exercises] ADD CONSTRAINT [FK_Exercises_LearningPathNodes_NodeId] FOREIGN KEY ([NodeId]) REFERENCES [LearningPathNodes] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260812061254_InitialCreate', N'10.0.11');

COMMIT;
GO

BEGIN TRANSACTION;
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ContentFeedback]') AND [c].[name] = N'Comment');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [ContentFeedback] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [ContentFeedback] ALTER COLUMN [Comment] nvarchar(1000) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260812180545_WidenContentFeedbackComment', N'10.0.11');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [OtpCodes] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [CodeHash] nvarchar(64) NOT NULL,
    [Purpose] nvarchar(32) NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [Used] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_OtpCodes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OtpCodes_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_OtpCodes_UserId] ON [OtpCodes] ([UserId]);

CREATE INDEX [IX_OtpCodes_UserId_Purpose_Used] ON [OtpCodes] ([UserId], [Purpose], [Used]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260812182232_AddOtpCodes', N'10.0.11');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Users] ADD [RowVersion] rowversion NULL;

ALTER TABLE [UserQuests] ADD [RowVersion] rowversion NULL;

ALTER TABLE [UserProgress] ADD [RowVersion] rowversion NULL;

ALTER TABLE [UserNodeProgress] ADD [RowVersion] rowversion NULL;

ALTER TABLE [UserInventory] ADD [RowVersion] rowversion NULL;

ALTER TABLE [PremiumSubscriptions] ADD [RowVersion] rowversion NULL;

ALTER TABLE [NodeSessions] ADD [RowVersion] rowversion NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260813051932_AddRowVersionConcurrency', N'10.0.11');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Users] ADD [Department] nvarchar(100) NULL;

ALTER TABLE [Users] ADD [StaffCode] nvarchar(50) NULL;

ALTER TABLE [Users] ADD [TeacherBio] nvarchar(500) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260813052933_AddTeacherProfileFields', N'10.0.11');

COMMIT;
GO

BEGIN TRANSACTION;
DELETE t FROM ExerciseSubmissions t
INNER JOIN (
    SELECT UserId, ExerciseId, ClassAssignmentId, MAX(Id) AS KeepId
    FROM ExerciseSubmissions
    WHERE ClassAssignmentId IS NOT NULL
    GROUP BY UserId, ExerciseId, ClassAssignmentId
    HAVING COUNT(*) > 1
) d ON t.UserId = d.UserId AND t.ExerciseId = d.ExerciseId
   AND t.ClassAssignmentId = d.ClassAssignmentId
   AND t.Id <> d.KeepId;

DELETE t FROM ExerciseSubmissions t
INNER JOIN (
    SELECT UserId, ExerciseId, MAX(Id) AS KeepId
    FROM ExerciseSubmissions
    WHERE ClassAssignmentId IS NULL
    GROUP BY UserId, ExerciseId
    HAVING COUNT(*) > 1
) d ON t.UserId = d.UserId AND t.ExerciseId = d.ExerciseId
   AND t.Id <> d.KeepId;

DELETE t FROM CodeSubmissions t
INNER JOIN (
    SELECT UserId, ExerciseId, MAX(Id) AS KeepId
    FROM CodeSubmissions
    GROUP BY UserId, ExerciseId
    HAVING COUNT(*) > 1
) d ON t.UserId = d.UserId AND t.ExerciseId = d.ExerciseId
   AND t.Id <> d.KeepId;

ALTER TABLE [CodeSubmissions] ADD [IsClientDeclared] bit NOT NULL DEFAULT CAST(1 AS bit);

CREATE UNIQUE INDEX [IX_ExerciseSubmissions_User_Exercise_Assignment] ON [ExerciseSubmissions] ([UserId], [ExerciseId], [ClassAssignmentId]) WHERE [ClassAssignmentId] IS NOT NULL;

CREATE UNIQUE INDEX [IX_ExerciseSubmissions_User_Exercise_NoAssignment] ON [ExerciseSubmissions] ([UserId], [ExerciseId]) WHERE [ClassAssignmentId] IS NULL;

CREATE UNIQUE INDEX [IX_CodeSubmissions_UserId_ExerciseId] ON [CodeSubmissions] ([UserId], [ExerciseId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260813064114_AddSubmissionUniqueConstraints', N'10.0.11');

COMMIT;
GO

BEGIN TRANSACTION;
DROP INDEX [IX_ExerciseSubmissions_User_Exercise_Assignment] ON [ExerciseSubmissions];

DROP INDEX [IX_ExerciseSubmissions_User_Exercise_NoAssignment] ON [ExerciseSubmissions];

DROP INDEX [IX_CodeSubmissions_UserId_ExerciseId] ON [CodeSubmissions];

ALTER TABLE [ExerciseSubmissions] ADD [ClientRequestId] nvarchar(128) NULL;

ALTER TABLE [CodeSubmissions] ADD [ClientRequestId] nvarchar(128) NULL;

CREATE UNIQUE INDEX [IX_ExerciseSubmissions_User_Exercise_Assignment_ClientRequestId] ON [ExerciseSubmissions] ([UserId], [ExerciseId], [ClassAssignmentId], [ClientRequestId]) WHERE [ClientRequestId] IS NOT NULL;

CREATE UNIQUE INDEX [IX_CodeSubmissions_User_Exercise_ClientRequestId] ON [CodeSubmissions] ([UserId], [ExerciseId], [ClientRequestId]) WHERE [ClientRequestId] IS NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260813070119_RemovePermanentSubmissionUnique', N'10.0.11');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE INDEX [IX_Users_LastActivityDate] ON [Users] ([LastActivityDate]);

CREATE INDEX [IX_Users_Xp] ON [Users] ([Xp]) WHERE [DeletedAt] IS NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260813075546_AddLeaderboardIndexes', N'10.0.11');

COMMIT;
GO

BEGIN TRANSACTION;
DELETE t FROM Lessons t
INNER JOIN (
    SELECT TopicId, Title, MAX(Id) AS KeepId
    FROM Lessons
    WHERE DeletedAt IS NULL
    GROUP BY TopicId, Title
    HAVING COUNT(*) > 1
) d ON t.TopicId = d.TopicId AND t.Title = d.Title
   AND t.Id <> d.KeepId
WHERE t.DeletedAt IS NULL;

DELETE t FROM Exercises t
INNER JOIN (
    SELECT LessonId, Title, MAX(Id) AS KeepId
    FROM Exercises
    WHERE DeletedAt IS NULL
    GROUP BY LessonId, Title
    HAVING COUNT(*) > 1
) d ON t.LessonId = d.LessonId AND t.Title = d.Title
   AND t.Id <> d.KeepId
WHERE t.DeletedAt IS NULL;

DELETE t FROM LearningPaths t
INNER JOIN (
    SELECT Title, MAX(Id) AS KeepId
    FROM LearningPaths
    GROUP BY Title
    HAVING COUNT(*) > 1
) d ON t.Title = d.Title
   AND t.Id <> d.KeepId;

DELETE t FROM LearningPathNodes t
INNER JOIN (
    SELECT PathId, Title, MAX(Id) AS KeepId
    FROM LearningPathNodes
    GROUP BY PathId, Title
    HAVING COUNT(*) > 1
) d ON t.PathId = d.PathId AND t.Title = d.Title
   AND t.Id <> d.KeepId;

DELETE t FROM LessonSimulations t
INNER JOIN (
    SELECT LessonId, SimulationKey, MAX(Id) AS KeepId
    FROM LessonSimulations
    GROUP BY LessonId, SimulationKey
    HAVING COUNT(*) > 1
) d ON t.LessonId = d.LessonId AND t.SimulationKey = d.SimulationKey
   AND t.Id <> d.KeepId;

DROP INDEX [IX_LessonSimulations_LessonId] ON [LessonSimulations];

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LessonSimulations]') AND [c].[name] = N'SimulationKey');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [LessonSimulations] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [LessonSimulations] ALTER COLUMN [SimulationKey] nvarchar(100) NOT NULL;

CREATE UNIQUE INDEX [IX_LessonSimulations_LessonId_SimulationKey] ON [LessonSimulations] ([LessonId], [SimulationKey]);

CREATE UNIQUE INDEX [IX_Lessons_TopicId_Title] ON [Lessons] ([TopicId], [Title]) WHERE [DeletedAt] IS NULL;

CREATE UNIQUE INDEX [IX_LearningPaths_Title] ON [LearningPaths] ([Title]);

CREATE UNIQUE INDEX [IX_LearningPathNodes_PathId_Title] ON [LearningPathNodes] ([PathId], [Title]);

CREATE UNIQUE INDEX [IX_Exercises_LessonId_Title] ON [Exercises] ([LessonId], [Title]) WHERE [DeletedAt] IS NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260813080807_AddContentUniqueIndexes', N'10.0.11');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Users] ADD [AcademicDegree] nvarchar(max) NULL;

ALTER TABLE [Users] ADD [ProfileLink] nvarchar(max) NULL;

ALTER TABLE [Lessons] ADD [IsClassOnly] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [Lessons] ADD [PublishedAt] datetime2 NULL;

ALTER TABLE [Lessons] ADD [RejectionReason] nvarchar(max) NULL;

ALTER TABLE [ClassAssignments] ADD [AllowLateSubmission] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [BugReports] ADD [AdminNote] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260814003835_FullBusinessLogicAndClassOverhaul', N'10.0.11');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [LearningPaths] ADD [AuthorId] int NULL;

ALTER TABLE [LearningPaths] ADD [HighlightsJson] nvarchar(max) NULL;

ALTER TABLE [LearningPaths] ADD [TestimonialsJson] nvarchar(max) NULL;

CREATE TABLE [CourseFeedback] (
    [Id] int NOT NULL IDENTITY,
    [CourseId] int NOT NULL,
    [UserId] int NOT NULL,
    [Type] int NOT NULL DEFAULT 0,
    [Content] nvarchar(1000) NOT NULL,
    [Status] int NOT NULL DEFAULT 0,
    [ReplyText] nvarchar(2000) NULL,
    [RepliedById] int NULL,
    [RepliedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_CourseFeedback] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CourseFeedback_LearningPaths_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [LearningPaths] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CourseFeedback_Users_RepliedById] FOREIGN KEY ([RepliedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CourseFeedback_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_LearningPaths_AuthorId] ON [LearningPaths] ([AuthorId]);

CREATE INDEX [IX_CourseFeedback_CourseId_CreatedAt] ON [CourseFeedback] ([CourseId], [CreatedAt]);

CREATE INDEX [IX_CourseFeedback_RepliedById] ON [CourseFeedback] ([RepliedById]);

CREATE INDEX [IX_CourseFeedback_Status_CreatedAt] ON [CourseFeedback] ([Status], [CreatedAt]);

CREATE INDEX [IX_CourseFeedback_UserId] ON [CourseFeedback] ([UserId]);

ALTER TABLE [LearningPaths] ADD CONSTRAINT [FK_LearningPaths_Users_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260814105726_AddCourseFeedbackAndCourseMarketing', N'10.0.11');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Classes] ADD [CurriculumDescription] nvarchar(500) NULL;

ALTER TABLE [Classes] ADD [CurriculumPublished] bit NOT NULL DEFAULT CAST(1 AS bit);

ALTER TABLE [Classes] ADD [CurriculumTitle] nvarchar(200) NULL;

ALTER TABLE [ClassAssignments] ADD [SortOrder] int NOT NULL DEFAULT 0;

CREATE INDEX [IX_ClassAssignments_ClassId_SortOrder] ON [ClassAssignments] ([ClassId], [SortOrder]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260817131632_AddClassCurriculum', N'10.0.11');

COMMIT;
GO

