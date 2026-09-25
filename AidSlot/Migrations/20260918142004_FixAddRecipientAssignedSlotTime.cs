using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AidSlot.Migrations
{
    public partial class FixAddRecipientAssignedSlotTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "AssignedSlotTime",
                table: "Recipients",
                type: "interval",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedSlotTime",
                table: "Recipients");
        }
    }
}