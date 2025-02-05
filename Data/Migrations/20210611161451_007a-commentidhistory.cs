using Microsoft.EntityFrameworkCore.Migrations;

namespace JamesonBugTracker.Data.Migrations
{
    public partial class _007acommentidhistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommentId",
                table: "TicketHistory",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommentId",
                table: "TicketHistory");
        }
    }
}
