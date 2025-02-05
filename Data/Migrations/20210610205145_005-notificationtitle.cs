using Microsoft.EntityFrameworkCore.Migrations;

namespace JamesonBugTracker.Data.Migrations
{
    public partial class _005notificationtitle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sender",
                table: "Notification");

            migrationBuilder.AlterColumn<string>(
                name: "SenderId",
                table: "Notification",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Notification",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notification_SenderId",
                table: "Notification",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_AspNetUsers_SenderId",
                table: "Notification",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notification_AspNetUsers_SenderId",
                table: "Notification");

            migrationBuilder.DropIndex(
                name: "IX_Notification_SenderId",
                table: "Notification");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Notification");

            migrationBuilder.AlterColumn<int>(
                name: "SenderId",
                table: "Notification",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Sender",
                table: "Notification",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
