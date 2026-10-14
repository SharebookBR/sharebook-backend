using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareBook.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class RetireFacilitatorFromBooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drops idempotentes: em producao o indice IX_Books_UserIdFacilitator nao existe e o
            // DROP INDEX direto derrubava o startup (Postgres 42704). Causa da divergencia: a apurar.
            migrationBuilder.Sql("ALTER TABLE \"Books\" DROP CONSTRAINT IF EXISTS \"FK_Books_Users_UserIdFacilitator\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Books_UserIdFacilitator\";");
            migrationBuilder.Sql("ALTER TABLE \"Books\" DROP COLUMN IF EXISTS \"UserIdFacilitator\";");

            // Preserva os comentarios admin: renomeia FacilitatorNotes -> AdminNotes (ou cria a coluna se nenhuma existir).
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Books' AND column_name = 'FacilitatorNotes')
       AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Books' AND column_name = 'AdminNotes') THEN
        ALTER TABLE ""Books"" RENAME COLUMN ""FacilitatorNotes"" TO ""AdminNotes"";
    ELSIF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Books' AND column_name = 'AdminNotes') THEN
        ALTER TABLE ""Books"" ADD COLUMN ""AdminNotes"" character varying(2000) NULL;
    END IF;
END $$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AdminNotes",
                table: "Books",
                newName: "FacilitatorNotes");

            migrationBuilder.AddColumn<Guid>(
                name: "UserIdFacilitator",
                table: "Books",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Books_UserIdFacilitator",
                table: "Books",
                column: "UserIdFacilitator");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Users_UserIdFacilitator",
                table: "Books",
                column: "UserIdFacilitator",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
