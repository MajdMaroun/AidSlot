using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AidSlot.Migrations
{
    /// <inheritdoc />
    public partial class MakeDocumentNumberUniquePerCampaign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Recipients_CampaignId",
                table: "Recipients");

            migrationBuilder.DropIndex(
                name: "IX_Recipients_DocumentNumber",
                table: "Recipients");

            migrationBuilder.CreateIndex(
                name: "IX_Recipients_CampaignId_DocumentNumber",
                table: "Recipients",
                columns: new[] { "CampaignId", "DocumentNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Recipients_CampaignId_DocumentNumber",
                table: "Recipients");

            migrationBuilder.CreateIndex(
                name: "IX_Recipients_CampaignId",
                table: "Recipients",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipients_DocumentNumber",
                table: "Recipients",
                column: "DocumentNumber",
                unique: true);
        }
    }
}
