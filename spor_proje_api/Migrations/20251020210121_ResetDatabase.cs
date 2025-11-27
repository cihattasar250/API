using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace spor_proje_api.Migrations
{
    /// <inheritdoc />
    public partial class ResetDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<int>(
                name: "SporcuId",
                table: "Performanslar",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "SporcuId",
                table: "Hedefler",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "SporcuId",
                table: "BeslenmeKayitlari",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "SporcuId",
                table: "Antrenmanlar",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Antrenmanlar_Uyeler_UyeId",
                table: "Antrenmanlar",
                column: "UyeId",
                principalTable: "Uyeler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BeslenmeKayitlari_Uyeler_UyeId",
                table: "BeslenmeKayitlari",
                column: "UyeId",
                principalTable: "Uyeler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Hedefler_Uyeler_UyeId",
                table: "Hedefler",
                column: "UyeId",
                principalTable: "Uyeler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Performanslar_Uyeler_UyeId",
                table: "Performanslar",
                column: "UyeId",
                principalTable: "Uyeler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.AlterColumn<int>(
                name: "SporcuId",
                table: "Performanslar",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SporcuId",
                table: "Hedefler",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SporcuId",
                table: "BeslenmeKayitlari",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SporcuId",
                table: "Antrenmanlar",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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
    }
}
