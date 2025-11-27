using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace spor_proje_api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sporcular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SporDali = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Yas = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Telefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Aktif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sporcular", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Antrenmanlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SporcuId = table.Column<int>(type: "int", nullable: false),
                    AntrenmanAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Sure = table.Column<int>(type: "int", nullable: false),
                    AntrenmanTipi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    KaloriYakilan = table.Column<int>(type: "int", nullable: false),
                    KalpAtisHizi = table.Column<int>(type: "int", nullable: false),
                    Notlar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Antrenmanlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Antrenmanlar_Sporcular_SporcuId",
                        column: x => x.SporcuId,
                        principalTable: "Sporcular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BeslenmeKayitlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SporcuId = table.Column<int>(type: "int", nullable: false),
                    YemekAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Ogun = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Kalori = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Protein = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Karbonhidrat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Yag = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeslenmeKayitlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeslenmeKayitlari_Sporcular_SporcuId",
                        column: x => x.SporcuId,
                        principalTable: "Sporcular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hedefler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SporcuId = table.Column<int>(type: "int", nullable: false),
                    HedefAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    HedefTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tamamlandi = table.Column<bool>(type: "bit", nullable: false),
                    TamamlanmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Kategori = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HedefDeger = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Birim = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hedefler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hedefler_Sporcular_SporcuId",
                        column: x => x.SporcuId,
                        principalTable: "Sporcular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Performanslar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SporcuId = table.Column<int>(type: "int", nullable: false),
                    PerformansAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Deger = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Birim = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Kategori = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Performanslar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Performanslar_Sporcular_SporcuId",
                        column: x => x.SporcuId,
                        principalTable: "Sporcular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Antrenmanlar_SporcuId",
                table: "Antrenmanlar",
                column: "SporcuId");

            migrationBuilder.CreateIndex(
                name: "IX_BeslenmeKayitlari_SporcuId",
                table: "BeslenmeKayitlari",
                column: "SporcuId");

            migrationBuilder.CreateIndex(
                name: "IX_Hedefler_SporcuId",
                table: "Hedefler",
                column: "SporcuId");

            migrationBuilder.CreateIndex(
                name: "IX_Performanslar_SporcuId",
                table: "Performanslar",
                column: "SporcuId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Antrenmanlar");

            migrationBuilder.DropTable(
                name: "BeslenmeKayitlari");

            migrationBuilder.DropTable(
                name: "Hedefler");

            migrationBuilder.DropTable(
                name: "Performanslar");

            migrationBuilder.DropTable(
                name: "Sporcular");
        }
    }
}
