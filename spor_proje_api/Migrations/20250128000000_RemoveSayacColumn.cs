using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace spor_proje_api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSayacColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sayac kolonunu kaldır (eğer varsa)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Uyeler]') AND name = 'Sayac')
                BEGIN
                    ALTER TABLE [Uyeler] DROP COLUMN [Sayac];
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri alma - Sayac kolonunu ekle
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Uyeler]') AND name = 'Sayac')
                BEGIN
                    ALTER TABLE [Uyeler] ADD [Sayac] BIGINT NOT NULL DEFAULT 0;
                END
            ");
        }
    }
}

