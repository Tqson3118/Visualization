using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DsaVisual.Application.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaderboardIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_LastActivityDate",
                table: "Users",
                column: "LastActivityDate");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Xp",
                table: "Users",
                column: "Xp",
                filter: "[DeletedAt] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_LastActivityDate",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Xp",
                table: "Users");
        }
    }
}
