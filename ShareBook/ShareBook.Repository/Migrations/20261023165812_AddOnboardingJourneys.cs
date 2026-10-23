using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareBook.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOnboardingJourneys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OnboardingJourneys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    NextStep = table.Column<int>(type: "integer", nullable: false),
                    NextEmailAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastQueuedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CanceledAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletionReason = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingJourneys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnboardingJourneys_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingJourneys_CompletedAtUtc_CanceledAtUtc_NextEmailAt~",
                table: "OnboardingJourneys",
                columns: new[] { "CompletedAtUtc", "CanceledAtUtc", "NextEmailAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingJourneys_UserId",
                table: "OnboardingJourneys",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OnboardingJourneys");
        }
    }
}
