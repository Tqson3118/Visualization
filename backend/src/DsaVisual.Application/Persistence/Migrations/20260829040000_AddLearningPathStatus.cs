using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DsaVisual.Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningPathStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "LearningPaths",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "LearningPaths",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedBy",
                table: "LearningPaths",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "LearningPaths",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "LearningPaths",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningPaths_ReviewedBy",
                table: "LearningPaths",
                column: "ReviewedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningPaths_Users_ReviewedBy",
                table: "LearningPaths",
                column: "ReviewedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Seed/existing data: ensure Status is Active (2)
            migrationBuilder.Sql("UPDATE LearningPaths SET Status = 2 WHERE Status = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningPaths_Users_ReviewedBy",
                table: "LearningPaths");

            migrationBuilder.DropIndex(
                name: "IX_LearningPaths_ReviewedBy",
                table: "LearningPaths");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "LearningPaths");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "LearningPaths");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                table: "LearningPaths");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "LearningPaths");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "LearningPaths");
        }
    }
}
