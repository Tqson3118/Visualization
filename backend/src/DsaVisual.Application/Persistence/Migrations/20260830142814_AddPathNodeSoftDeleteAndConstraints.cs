using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DsaVisual.Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPathNodeSoftDeleteAndConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LearningPathNodes_PathId_SortOrder' AND object_id = OBJECT_ID('LearningPathNodes'))
                    DROP INDEX IX_LearningPathNodes_PathId_SortOrder ON LearningPathNodes;

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LearningPathNodes_PathId_Title' AND object_id = OBJECT_ID('LearningPathNodes'))
                    DROP INDEX IX_LearningPathNodes_PathId_Title ON LearningPathNodes;

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Lessons_TopicId_Title' AND is_unique = 1 AND object_id = OBJECT_ID('Lessons'))
                    DROP INDEX IX_Lessons_TopicId_Title ON Lessons;

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LearningPaths_Title' AND is_unique = 1 AND object_id = OBJECT_ID('LearningPaths'))
                    DROP INDEX IX_LearningPaths_Title ON LearningPaths;

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ClassAssignments_ClassId_PathItemId' AND object_id = OBJECT_ID('ClassAssignments'))
                    DROP INDEX IX_ClassAssignments_ClassId_PathItemId ON ClassAssignments;

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'DeletedAt' AND object_id = OBJECT_ID('LearningPathNodes'))
                    ALTER TABLE LearningPathNodes ADD DeletedAt datetime2 NULL;

                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LearningPathNodes_PathId_ParentId_SortOrder' AND object_id = OBJECT_ID('LearningPathNodes'))
                    CREATE NONCLUSTERED INDEX IX_LearningPathNodes_PathId_ParentId_SortOrder ON LearningPathNodes (PathId, ParentId, SortOrder);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE name = 'DeletedAt' AND object_id = OBJECT_ID('LearningPathNodes'))
                    ALTER TABLE LearningPathNodes DROP COLUMN DeletedAt;
            ");
        }
    }
}
