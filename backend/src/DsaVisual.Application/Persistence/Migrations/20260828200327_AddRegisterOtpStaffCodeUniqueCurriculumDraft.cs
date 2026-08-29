using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DsaVisual.Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRegisterOtpStaffCodeUniqueCurriculumDraft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "CurriculumPublished",
                table: "Classes",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.CreateTable(
                name: "RegisterOtpCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CodeHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    VerifyTokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedAttempts = table.Column<int>(type: "int", nullable: false),
                    Used = table.Column<bool>(type: "bit", nullable: false),
                    TokenUsed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisterOtpCodes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_StaffCode",
                table: "Users",
                column: "StaffCode",
                unique: true,
                filter: "[StaffCode] IS NOT NULL AND [DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RegisterOtpCodes_Email_Used",
                table: "RegisterOtpCodes",
                columns: new[] { "Email", "Used" });

            migrationBuilder.CreateIndex(
                name: "IX_RegisterOtpCodes_VerifyTokenHash",
                table: "RegisterOtpCodes",
                column: "VerifyTokenHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegisterOtpCodes");

            migrationBuilder.DropIndex(
                name: "IX_Users_StaffCode",
                table: "Users");

            migrationBuilder.AlterColumn<bool>(
                name: "CurriculumPublished",
                table: "Classes",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);
        }
    }
}
