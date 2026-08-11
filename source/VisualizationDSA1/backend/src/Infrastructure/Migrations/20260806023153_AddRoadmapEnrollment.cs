using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoadmapEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoadmapEnrollments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoadmapId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    EnrolledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoadmapEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoadmapEnrollments_CustomRoadmaps_RoadmapId",
                        column: x => x.RoadmapId,
                        principalTable: "CustomRoadmaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoadmapEnrollments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapEnrollments_RoadmapId",
                table: "RoadmapEnrollments",
                column: "RoadmapId");

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapEnrollments_UserId_RoadmapId",
                table: "RoadmapEnrollments",
                columns: new[] { "UserId", "RoadmapId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoadmapEnrollments_UserId_Status",
                table: "RoadmapEnrollments",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoadmapEnrollments");
        }
    }
}
