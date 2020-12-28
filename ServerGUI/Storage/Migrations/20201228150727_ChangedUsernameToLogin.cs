using Microsoft.EntityFrameworkCore.Migrations;

namespace Storage.Migrations
{
    public partial class ChangedUsernameToLogin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Username",
                table: "UserDatas",
                newName: "Login");

            migrationBuilder.RenameIndex(
                name: "IX_UserDatas_Username",
                table: "UserDatas",
                newName: "IX_UserDatas_Login");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Login",
                table: "UserDatas",
                newName: "Username");

            migrationBuilder.RenameIndex(
                name: "IX_UserDatas_Login",
                table: "UserDatas",
                newName: "IX_UserDatas_Username");
        }
    }
}
