using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JamesonBugTracker.Data.Migrations
{
    public partial class _009completedticketdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Ticket",
                type: "character varying(75)",
                maxLength: 75,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Completed",
                table: "Ticket",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Completed",
                table: "Ticket");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Ticket",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(75)",
                oldMaxLength: 75);
        }
    }
}
