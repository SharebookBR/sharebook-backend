using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareBook.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameEFLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename in place — preserva as linhas existentes de auditoria (LogEntries vinha sendo
            // usada pelo LGPD para expurgo de log por usuário/endereço removido).
            migrationBuilder.RenameTable(
                name: "LogEntries",
                newName: "EFLogs");

            migrationBuilder.RenameIndex(
                table: "EFLogs",
                name: "IX_LogEntries_EntityName_EntityId",
                newName: "IX_EFLogs_EntityName_EntityId");

            migrationBuilder.Sql(
                "ALTER TABLE \"EFLogs\" RENAME CONSTRAINT \"PK_LogEntries\" TO \"PK_EFLogs\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"EFLogs\" RENAME CONSTRAINT \"PK_EFLogs\" TO \"PK_LogEntries\";");

            migrationBuilder.RenameIndex(
                table: "EFLogs",
                name: "IX_EFLogs_EntityName_EntityId",
                newName: "IX_LogEntries_EntityName_EntityId");

            migrationBuilder.RenameTable(
                name: "EFLogs",
                newName: "LogEntries");
        }
    }
}
