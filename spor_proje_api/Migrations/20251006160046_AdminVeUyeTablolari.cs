using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace spor_proje_api.Migrations
{
    /// <inheritdoc />
    public partial class AdminVeUyeTablolari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UyeId",
                table: "Performanslar",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UyeId",
                table: "Hedefler",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UyeId",
                table: "BeslenmeKayitlari",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UyeId",
                table: "Antrenmanlar",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Adminler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AdminNumarasi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Sifre = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    SonGirisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adminler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Uyeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DogumTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cinsiyet = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Adres = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AcilDurumIletisim = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UyelikTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UyeNumarasi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Sifre = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    SonGirisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uyeler", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Performanslar_UyeId",
                table: "Performanslar",
                column: "UyeId");

            migrationBuilder.CreateIndex(
                name: "IX_Hedefler_UyeId",
                table: "Hedefler",
                column: "UyeId");

            migrationBuilder.CreateIndex(
                name: "IX_BeslenmeKayitlari_UyeId",
                table: "BeslenmeKayitlari",
                column: "UyeId");

            migrationBuilder.CreateIndex(
                name: "IX_Antrenmanlar_UyeId",
                table: "Antrenmanlar",
                column: "UyeId");

            migrationBuilder.CreateIndex(
                name: "IX_Adminler_AdminNumarasi",
                table: "Adminler",
                column: "AdminNumarasi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Adminler_Email",
                table: "Adminler",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Uyeler_Email",
                table: "Uyeler",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Uyeler_UyeNumarasi",
                table: "Uyeler",
                column: "UyeNumarasi",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Antrenmanlar_Uyeler_UyeId",
                table: "Antrenmanlar",
                column: "UyeId",
                principalTable: "Uyeler",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BeslenmeKayitlari_Uyeler_UyeId",
                table: "BeslenmeKayitlari",
                column: "UyeId",
                principalTable: "Uyeler",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Hedefler_Uyeler_UyeId",
                table: "Hedefler",
                column: "UyeId",
                principalTable: "Uyeler",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Performanslar_Uyeler_UyeId",
                table: "Performanslar",
                column: "UyeId",
                principalTable: "Uyeler",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Antrenmanlar_Uyeler_UyeId",
                table: "Antrenmanlar");

            migrationBuilder.DropForeignKey(
                name: "FK_BeslenmeKayitlari_Uyeler_UyeId",
                table: "BeslenmeKayitlari");

            migrationBuilder.DropForeignKey(
                name: "FK_Hedefler_Uyeler_UyeId",
                table: "Hedefler");

            migrationBuilder.DropForeignKey(
                name: "FK_Performanslar_Uyeler_UyeId",
                table: "Performanslar");

            migrationBuilder.DropTable(
                name: "Adminler");

            migrationBuilder.DropTable(
                name: "Uyeler");

            migrationBuilder.DropIndex(
                name: "IX_Performanslar_UyeId",
                table: "Performanslar");

            migrationBuilder.DropIndex(
                name: "IX_Hedefler_UyeId",
                table: "Hedefler");

            migrationBuilder.DropIndex(
                name: "IX_BeslenmeKayitlari_UyeId",
                table: "BeslenmeKayitlari");

            migrationBuilder.DropIndex(
                name: "IX_Antrenmanlar_UyeId",
                table: "Antrenmanlar");

            migrationBuilder.DropColumn(
                name: "UyeId",
                table: "Performanslar");

            migrationBuilder.DropColumn(
                name: "UyeId",
                table: "Hedefler");

            migrationBuilder.DropColumn(
                name: "UyeId",
                table: "BeslenmeKayitlari");

            migrationBuilder.DropColumn(
                name: "UyeId",
                table: "Antrenmanlar");
        }
    }
}
