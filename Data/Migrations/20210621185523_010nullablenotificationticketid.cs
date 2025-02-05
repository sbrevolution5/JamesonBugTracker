using Microsoft.EntityFrameworkCore.Migrations;

namespace JamesonBugTracker.Data.Migrations
{
    public partial class _010nullablenotificationticketid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notification_Ticket_TicketId",
                table: "Notification");

            migrationBuilder.AlterColumn<int>(
                name: "TicketId",
                table: "Notification",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_Ticket_TicketId",
                table: "Notification",
                column: "TicketId",
                principalTable: "Ticket",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notification_Ticket_TicketId",
                table: "Notification");

            migrationBuilder.AlterColumn<int>(
                name: "TicketId",
                table: "Notification",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_Ticket_TicketId",
                table: "Notification",
                column: "TicketId",
                principalTable: "Ticket",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
