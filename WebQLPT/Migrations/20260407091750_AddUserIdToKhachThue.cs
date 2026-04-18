using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebQLPT.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToKhachThue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "KhachThues",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_KhachThues_UserId",
                table: "KhachThues",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_KhachThues_AspNetUsers_UserId",
                table: "KhachThues",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KhachThues_AspNetUsers_UserId",
                table: "KhachThues");

            migrationBuilder.DropIndex(
                name: "IX_KhachThues_UserId",
                table: "KhachThues");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "KhachThues");
        }
    }
}
