using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebQLPT.Migrations
{
    /// <inheritdoc />
    public partial class AddDangTin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DangTins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TieuDe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayDang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PhongTroId = table.Column<int>(type: "int", nullable: false),
                    ChuTroId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangTins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DangTins_ChuTros_ChuTroId",
                        column: x => x.ChuTroId,
                        principalTable: "ChuTros",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DangTins_PhongTros_PhongTroId",
                        column: x => x.PhongTroId,
                        principalTable: "PhongTros",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DangTins_ChuTroId",
                table: "DangTins",
                column: "ChuTroId");

            migrationBuilder.CreateIndex(
                name: "IX_DangTins_PhongTroId",
                table: "DangTins",
                column: "PhongTroId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DangTins");
        }
    }
}
