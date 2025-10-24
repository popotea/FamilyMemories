using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyMemories.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserToMemory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Memories",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Memories_ApplicationUserId",
                table: "Memories",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Memories_AspNetUsers_ApplicationUserId",
                table: "Memories",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memories_AspNetUsers_ApplicationUserId",
                table: "Memories");

            migrationBuilder.DropIndex(
                name: "IX_Memories_ApplicationUserId",
                table: "Memories");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Memories");
        }
    }
}
