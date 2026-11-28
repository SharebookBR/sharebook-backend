using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareBook.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLegacyOnboardingRollout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LegacyOnboardingBatchId",
                table: "OnboardingJourneys",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LegacyOnboardingBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    ExecutedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EnrolledCount = table.Column<int>(type: "integer", nullable: false),
                    NewestUserCreationAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OldestUserCreationAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegacyOnboardingBatches", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OnboardingJourneys_LegacyOnboardingBatchId",
                table: "OnboardingJourneys",
                column: "LegacyOnboardingBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_LegacyOnboardingBatches_ExecutedAtUtc",
                table: "LegacyOnboardingBatches",
                column: "ExecutedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_LegacyOnboardingBatches_SequenceNumber",
                table: "LegacyOnboardingBatches",
                column: "SequenceNumber",
                unique: true);

            // As duas primeiras remessas foram registradas apenas no histórico do job.
            // Reconstituímos seu contexto uma única vez, preservando o tamanho original
            // da remessa mesmo que alguma conta já tenha sido removida desde então.
            migrationBuilder.Sql("""
                WITH history AS (
                    SELECT
                        h."Id",
                        h."CreationDate",
                        h."Details",
                        row_number() OVER (ORDER BY h."CreationDate") AS "SequenceNumber"
                    FROM "JobHistories" h
                    WHERE h."JobName" = 'LegacyOnboardingBackfill'
                      AND h."IsSuccess" = true
                      AND h."Details" LIKE 'Lote legado criado:%'
                )
                INSERT INTO "LegacyOnboardingBatches" (
                    "Id", "SequenceNumber", "ExecutedAtUtc", "EnrolledCount",
                    "NewestUserCreationAtUtc", "OldestUserCreationAtUtc", "CreationDate")
                SELECT
                    md5('LegacyOnboardingBackfill:' || h."Id"::text)::uuid,
                    h."SequenceNumber",
                    h."CreationDate",
                    (regexp_match(h."Details", 'Lote legado criado: ([0-9]+) jornada'))[1]::integer,
                    ((regexp_match(h."Details", 'Janela de cadastro: ([0-9-]{10} [0-9:]{8}) UTC até ([0-9-]{10} [0-9:]{8}) UTC'))[1]::timestamp AT TIME ZONE 'UTC'),
                    ((regexp_match(h."Details", 'Janela de cadastro: ([0-9-]{10} [0-9:]{8}) UTC até ([0-9-]{10} [0-9:]{8}) UTC'))[2]::timestamp AT TIME ZONE 'UTC'),
                    h."CreationDate"
                FROM history h;

                UPDATE "OnboardingJourneys" journey
                SET "LegacyOnboardingBatchId" = batch."Id"
                FROM "LegacyOnboardingBatches" batch
                WHERE journey."LegacyOnboardingBatchId" IS NULL
                  AND journey."CreationDate" >= batch."ExecutedAtUtc" - interval '5 seconds'
                  AND journey."CreationDate" <= batch."ExecutedAtUtc";
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_OnboardingJourneys_LegacyOnboardingBatches_LegacyOnboarding~",
                table: "OnboardingJourneys",
                column: "LegacyOnboardingBatchId",
                principalTable: "LegacyOnboardingBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OnboardingJourneys_LegacyOnboardingBatches_LegacyOnboarding~",
                table: "OnboardingJourneys");

            migrationBuilder.DropTable(
                name: "LegacyOnboardingBatches");

            migrationBuilder.DropIndex(
                name: "IX_OnboardingJourneys_LegacyOnboardingBatchId",
                table: "OnboardingJourneys");

            migrationBuilder.DropColumn(
                name: "LegacyOnboardingBatchId",
                table: "OnboardingJourneys");
        }
    }
}
