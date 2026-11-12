using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareBook.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOnboardingEmailTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OnboardingEmailDeliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OnboardingJourneyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Step = table.Column<int>(type: "integer", nullable: false),
                    QueuedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FirstOpenedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingEmailDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnboardingEmailDeliveries_OnboardingJourneys_OnboardingJour~",
                        column: x => x.OnboardingJourneyId,
                        principalTable: "OnboardingJourneys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OnboardingEmailClicks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OnboardingEmailDeliveryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cta = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FirstClickedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingEmailClicks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnboardingEmailClicks_OnboardingEmailDeliveries_OnboardingE~",
                        column: x => x.OnboardingEmailDeliveryId,
                        principalTable: "OnboardingEmailDeliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingEmailClicks_OnboardingEmailDeliveryId_Cta",
                table: "OnboardingEmailClicks",
                columns: new[] { "OnboardingEmailDeliveryId", "Cta" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingEmailDeliveries_OnboardingJourneyId_Step",
                table: "OnboardingEmailDeliveries",
                columns: new[] { "OnboardingJourneyId", "Step" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingEmailDeliveries_QueuedAtUtc_Step",
                table: "OnboardingEmailDeliveries",
                columns: new[] { "QueuedAtUtc", "Step" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OnboardingEmailClicks");

            migrationBuilder.DropTable(
                name: "OnboardingEmailDeliveries");
        }
    }
}
