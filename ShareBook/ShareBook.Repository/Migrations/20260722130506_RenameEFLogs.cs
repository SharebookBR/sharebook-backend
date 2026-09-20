using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace ShareBook.Infra.Data.Migrations;

/// <inheritdoc />
public partial class RenameEFLogs : Migration
{
    // Produção (banco antigo, migrado de SQL Server) tem os nomes reais com prefixo
    // "idx_17657_" — herança de uma ferramenta de port que prefixou constraint/index, não a
    // tabela. Mas um banco criado do zero por MigrationInicialPostgres usa a convenção padrão
    // do EF (PK_LogEntries / IX_LogEntries_EntityName_EntityId), sem esse prefixo. O RENAME
    // por SQL condicional abaixo cobre os dois casos, em vez de assumir só o nome de produção
    // (confirmado quebrando num Postgres limpo durante a Tarefa 4 do épico de simplificação).
    private const string LegacyPrimaryKeyName = "idx_17657_PK_LogEntries";
    private const string StandardPrimaryKeyName = "PK_LogEntries";
    private const string LegacyIndexName = "idx_17657_IX_LogEntries_EntityName_EntityId";
    private const string StandardIndexName = "IX_LogEntries_EntityName_EntityId";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Rename in place — preserva as linhas existentes de auditoria (LogEntries vinha sendo
        // usada pelo LGPD para expurgo de log por usuário/endereço removido).
        migrationBuilder.RenameTable(
            name: "LogEntries",
            newName: "EFLogs");

        migrationBuilder.Sql($"""
            DO $$
            BEGIN
                IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = '{LegacyIndexName}') THEN
                    ALTER INDEX "{LegacyIndexName}" RENAME TO "IX_EFLogs_EntityName_EntityId";
                ELSIF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = '{StandardIndexName}') THEN
                    ALTER INDEX "{StandardIndexName}" RENAME TO "IX_EFLogs_EntityName_EntityId";
                END IF;
            END $$;
            """);

        migrationBuilder.Sql($"""
            DO $$
            BEGIN
                IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = '{LegacyPrimaryKeyName}') THEN
                    ALTER TABLE "EFLogs" RENAME CONSTRAINT "{LegacyPrimaryKeyName}" TO "PK_EFLogs";
                ELSIF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = '{StandardPrimaryKeyName}') THEN
                    ALTER TABLE "EFLogs" RENAME CONSTRAINT "{StandardPrimaryKeyName}" TO "PK_EFLogs";
                END IF;
            END $$;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            $"ALTER TABLE \"EFLogs\" RENAME CONSTRAINT \"PK_EFLogs\" TO \"{StandardPrimaryKeyName}\";");

        migrationBuilder.RenameIndex(
            table: "EFLogs",
            name: "IX_EFLogs_EntityName_EntityId",
            newName: StandardIndexName);

        migrationBuilder.RenameTable(
            name: "EFLogs",
            newName: "LogEntries");
    }
}
