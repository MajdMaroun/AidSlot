using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AidSlot.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipientReceivingStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasReceived",
                table: "Recipients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedAt",
                table: "Recipients",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasReceived",
                table: "Recipients");

            migrationBuilder.DropColumn(
                name: "ReceivedAt",
                table: "Recipients");
        }
    }
}