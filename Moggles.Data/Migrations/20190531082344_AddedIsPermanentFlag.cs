using Microsoft.EntityFrameworkCore.Migrations;

namespace Moggles.Data.Migrations
{
    public partial class AddedIsPermanentFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPermanent",
                table: "FeatureToggles",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPermanent",
                table: "FeatureToggles");
        }
    }
}
