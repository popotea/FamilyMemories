using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyMemories.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleAndUserPermissionsJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserPermissions",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RolePermissions",
                table: "AspNetRoles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserPermissions",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RolePermissions",
                table: "AspNetRoles");
        }
    }
}
