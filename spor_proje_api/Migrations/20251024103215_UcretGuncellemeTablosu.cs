using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace spor_proje_api.Migrations
{
    /// <inheritdoc />
    public partial class UcretGuncellemeTablosu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UyelikTuru",
                table: "Uyeler",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<decimal>(
                name: "UyelikUcreti",
                table: "Uyeler",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "UcretGuncellemeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UyeId = table.Column<int>(type: "int", nullable: false),
                    EskiUcret = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    YeniUcret = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ArtisNedeni = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    GecerlilikTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    GuncelleyenAdminId = table.Column<int>(type: "int", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UcretGuncellemeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UcretGuncellemeleri_Adminler_GuncelleyenAdminId",
                        column: x => x.GuncelleyenAdminId,
                        principalTable: "Adminler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UcretGuncellemeleri_Uyeler_UyeId",
                        column: x => x.UyeId,
                        principalTable: "Uyeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UcretGuncellemeleri_GuncelleyenAdminId",
                table: "UcretGuncellemeleri",
                column: "GuncelleyenAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_UcretGuncellemeleri_UyeId",
                table: "UcretGuncellemeleri",
                column: "UyeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UcretGuncellemeleri");

            migrationBuilder.DropColumn(
                name: "UyelikUcreti",
                table: "Uyeler");

            migrationBuilder.AlterColumn<string>(
                name: "UyelikTuru",
                table: "Uyeler",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
