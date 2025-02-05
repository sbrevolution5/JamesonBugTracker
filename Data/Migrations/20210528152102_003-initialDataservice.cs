using Microsoft.EntityFrameworkCore.Migrations;

namespace JamesonBugTracker.Data.Migrations
{
    public partial class _003initialDataservice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Priority",
                table: "ProjectPriority",
                newName: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ProjectPriority",
                newName: "Priority");
        }
    }
}
