using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DsaVisual.Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClassCurriculum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurriculumDescription",
                table: "Classes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CurriculumPublished",
                table: "Classes",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "CurriculumTitle",
                table: "Classes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "ClassAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ClassAssignments_ClassId_SortOrder",
                table: "ClassAssignments",
                columns: new[] { "ClassId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClassAssignments_ClassId_SortOrder",
                table: "ClassAssignments");

            migrationBuilder.DropColumn(
                name: "CurriculumDescription",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "CurriculumPublished",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "CurriculumTitle",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "ClassAssignments");
        }
    }
}
