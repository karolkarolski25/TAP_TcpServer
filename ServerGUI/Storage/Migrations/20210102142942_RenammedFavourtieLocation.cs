using Microsoft.EntityFrameworkCore.Migrations;

namespace Storage.Migrations
{
    public partial class RenammedFavourtieLocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FavouriteLocation",
                table: "Users",
                newName: "FavouriteLocations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FavouriteLocations",
                table: "Users",
                newName: "FavouriteLocation");
        }
    }
}
