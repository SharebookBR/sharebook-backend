using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareBook.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameEFLogs : Migration
    {
        // Nomes reais em produção têm prefixo "idx_17657_" — herança da migração histórica de
        // SQL Server para Postgres (ferramenta de port prefixou constraint/index, não a tabela).
        // Confirmado via pg_indexes/pg_constraint antes de corrigir; convenção do EF não bate
        // com a realidade física para objetos criados antes desse port.
        private const string LegacyPrimaryKeyName = "idx_17657_PK_LogEntries";
        private const string LegacyIndexName = "idx_17657_IX_LogEntries_EntityName_EntityId";

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
                name: LegacyIndexName,
                newName: "IX_EFLogs_EntityName_EntityId");

            migrationBuilder.Sql(
                $"ALTER TABLE \"EFLogs\" RENAME CONSTRAINT \"{LegacyPrimaryKeyName}\" TO \"PK_EFLogs\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $"ALTER TABLE \"EFLogs\" RENAME CONSTRAINT \"PK_EFLogs\" TO \"{LegacyPrimaryKeyName}\";");

            migrationBuilder.RenameIndex(
                table: "EFLogs",
                name: "IX_EFLogs_EntityName_EntityId",
                newName: LegacyIndexName);

            migrationBuilder.RenameTable(
                name: "EFLogs",
                newName: "LogEntries");
        }
    }
}
