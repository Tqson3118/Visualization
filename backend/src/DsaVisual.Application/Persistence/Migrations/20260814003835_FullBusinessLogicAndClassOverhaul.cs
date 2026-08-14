using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DsaVisual.Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FullBusinessLogicAndClassOverhaul : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AcademicDegree",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileLink",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsClassOnly",
                table: "Lessons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "Lessons",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Lessons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowLateSubmission",
                table: "ClassAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AdminNote",
                table: "BugReports",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcademicDegree",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfileLink",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsClassOnly",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "AllowLateSubmission",
                table: "ClassAssignments");

            migrationBuilder.DropColumn(
                name: "AdminNote",
                table: "BugReports");
        }
    }
}
