using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace spor_proje_api.Migrations
{
    /// <inheritdoc />
    public partial class PerformansTakibiTablosu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PerformansTakibi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UyeId = table.Column<int>(type: "int", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Kilo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VucutYagOrani = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    KasKutlesi = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    KardiyoSuresi = table.Column<int>(type: "int", nullable: true),
                    Notlar = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformansTakibi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerformansTakibi_Uyeler_UyeId",
                        column: x => x.UyeId,
                        principalTable: "Uyeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PerformansTakibi_UyeId_Tarih",
                table: "PerformansTakibi",
                columns: new[] { "UyeId", "Tarih" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerformansTakibi");
        }
    }
}
