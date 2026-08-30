using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DsaVisual.Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningPathVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LearningPathNodes_PathId_SortOrder",
                table: "LearningPathNodes");

            migrationBuilder.DropIndex(
                name: "IX_LearningPathNodes_PathId_Title",
                table: "LearningPathNodes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ClassAssignments_Content",
                table: "ClassAssignments");

            migrationBuilder.AddColumn<int>(
                name: "Visibility",
                table: "LearningPaths",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "LearningPathNodes",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemType",
                table: "LearningPathNodes",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "LabExerciseId",
                table: "LearningPathNodes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "LearningPathNodes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LearningPathId",
                table: "Classes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Archived",
                table: "ClassAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PathItemId",
                table: "ClassAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningPathNodes_LabExerciseId",
                table: "LearningPathNodes",
                column: "LabExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPathNodes_ParentId",
                table: "LearningPathNodes",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPathNodes_PathId_ParentId_SortOrder",
                table: "LearningPathNodes",
                columns: new[] { "PathId", "ParentId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Classes_LearningPathId",
                table: "Classes",
                column: "LearningPathId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassAssignments_ClassId_PathItemId",
                table: "ClassAssignments",
                columns: new[] { "ClassId", "PathItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassAssignments_PathItemId",
                table: "ClassAssignments",
                column: "PathItemId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ClassAssignments_Content",
                table: "ClassAssignments",
                sql: "([LessonId] IS NOT NULL OR [ExerciseId] IS NOT NULL OR [PathItemId] IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassAssignments_LearningPathNodes_PathItemId",
                table: "ClassAssignments",
                column: "PathItemId",
                principalTable: "LearningPathNodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_LearningPaths_LearningPathId",
                table: "Classes",
                column: "LearningPathId",
                principalTable: "LearningPaths",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningPathNodes_Exercises_LabExerciseId",
                table: "LearningPathNodes",
                column: "LabExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningPathNodes_LearningPathNodes_ParentId",
                table: "LearningPathNodes",
                column: "ParentId",
                principalTable: "LearningPathNodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassAssignments_LearningPathNodes_PathItemId",
                table: "ClassAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Classes_LearningPaths_LearningPathId",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningPathNodes_Exercises_LabExerciseId",
                table: "LearningPathNodes");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningPathNodes_LearningPathNodes_ParentId",
                table: "LearningPathNodes");

            migrationBuilder.DropIndex(
                name: "IX_LearningPathNodes_LabExerciseId",
                table: "LearningPathNodes");

            migrationBuilder.DropIndex(
                name: "IX_LearningPathNodes_ParentId",
                table: "LearningPathNodes");

            migrationBuilder.DropIndex(
                name: "IX_LearningPathNodes_PathId_ParentId_SortOrder",
                table: "LearningPathNodes");

            migrationBuilder.DropIndex(
                name: "IX_Classes_LearningPathId",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_ClassAssignments_ClassId_PathItemId",
                table: "ClassAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ClassAssignments_PathItemId",
                table: "ClassAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ClassAssignments_Content",
                table: "ClassAssignments");

            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "LearningPaths");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "LearningPathNodes");

            migrationBuilder.DropColumn(
                name: "ItemType",
                table: "LearningPathNodes");

            migrationBuilder.DropColumn(
                name: "LabExerciseId",
                table: "LearningPathNodes");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "LearningPathNodes");

            migrationBuilder.DropColumn(
                name: "LearningPathId",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "Archived",
                table: "ClassAssignments");

            migrationBuilder.DropColumn(
                name: "PathItemId",
                table: "ClassAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPathNodes_PathId_SortOrder",
                table: "LearningPathNodes",
                columns: new[] { "PathId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningPathNodes_PathId_Title",
                table: "LearningPathNodes",
                columns: new[] { "PathId", "Title" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ClassAssignments_Content",
                table: "ClassAssignments",
                sql: "([LessonId] IS NOT NULL OR [ExerciseId] IS NOT NULL)");
        }
    }
}
