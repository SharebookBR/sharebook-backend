using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareBook.Repository.Migrations
{
    /// <summary>
    /// Migration para atualizar campos de e-book:
    /// - Remove: EBookDownloadLink, EBookPdfFile, EBookPdfBytes
    /// - Adiciona: EBookPdfPath (caminho do PDF usando slug)
    /// </summary>
    public partial class UpdateEBookFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove colunas antigas (se existirem)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Books' AND COLUMN_NAME = 'EBookDownloadLink')
                BEGIN
                    ALTER TABLE Books DROP COLUMN EBookDownloadLink;
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Books' AND COLUMN_NAME = 'EBookPdfFile')
                BEGIN
                    ALTER TABLE Books DROP COLUMN EBookPdfFile;
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Books' AND COLUMN_NAME = 'EBookPdfBytes')
                BEGIN
                    ALTER TABLE Books DROP COLUMN EBookPdfBytes;
                END
            ");

            // Adiciona nova coluna EBookPdfPath (se não existir)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Books' AND COLUMN_NAME = 'EBookPdfPath')
                BEGIN
                    ALTER TABLE Books ADD EBookPdfPath nvarchar(500) NULL;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove nova coluna
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Books' AND COLUMN_NAME = 'EBookPdfPath')
                BEGIN
                    ALTER TABLE Books DROP COLUMN EBookPdfPath;
                END
            ");

            // Recria colunas antigas
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Books' AND COLUMN_NAME = 'EBookDownloadLink')
                BEGIN
                    ALTER TABLE Books ADD EBookDownloadLink nvarchar(max) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Books' AND COLUMN_NAME = 'EBookPdfFile')
                BEGIN
                    ALTER TABLE Books ADD EBookPdfFile nvarchar(max) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Books' AND COLUMN_NAME = 'EBookPdfBytes')
                BEGIN
                    ALTER TABLE Books ADD EBookPdfBytes varbinary(max) NULL;
                END
            ");
        }
    }
}
