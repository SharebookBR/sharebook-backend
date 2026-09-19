using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareBook.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnforceUniqueBookSlugs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Keep the row currently resolved by BySlugAsync (newest CreationDate) on the
            // existing public URL. Older colliding rows receive the first available _copyN
            // variant, preserving the catalog's established URL format.
            migrationBuilder.Sql(
                """
                DO $$
                DECLARE
                    duplicate_book RECORD;
                    base_slug TEXT;
                    copy_number INTEGER;
                    suffix TEXT;
                    candidate TEXT;
                BEGIN
                    FOR duplicate_book IN
                        SELECT "Id", "Slug", resolution_rank
                        FROM (
                            SELECT
                                "Id",
                                "Slug",
                                ROW_NUMBER() OVER (
                                    PARTITION BY "Slug"
                                    ORDER BY "CreationDate" DESC NULLS FIRST, "Id" DESC
                                ) AS resolution_rank
                            FROM "Books"
                            WHERE "Slug" IS NOT NULL
                        ) AS ranked
                        WHERE resolution_rank > 1
                        ORDER BY "Slug", resolution_rank
                    LOOP
                        base_slug := REGEXP_REPLACE(duplicate_book."Slug", '_copy[0-9]+$', '');
                        copy_number := 0;

                        LOOP
                            IF copy_number = 0 THEN
                                candidate := base_slug;
                            ELSE
                                suffix := '_copy' || copy_number;
                                candidate := LEFT(base_slug, 100 - LENGTH(suffix)) || suffix;
                            END IF;

                            EXIT WHEN NOT EXISTS (
                                SELECT 1
                                FROM "Books"
                                WHERE "Slug" = candidate
                                  AND "Id" <> duplicate_book."Id"
                            );

                            copy_number := copy_number + 1;
                        END LOOP;

                        UPDATE "Books"
                        SET "Slug" = candidate
                        WHERE "Id" = duplicate_book."Id";
                    END LOOP;
                END $$;
                """);

            migrationBuilder.CreateIndex(
                name: "UX_Books_Slug",
                table: "Books",
                column: "Slug",
                unique: true,
                filter: "\"Slug\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Books_Slug",
                table: "Books");
        }
    }
}
