using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AidSlot.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignCsvFileName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CsvFileName",
                table: "Campaigns",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CsvFileName",
                table: "Campaigns");
        }
    }
}
