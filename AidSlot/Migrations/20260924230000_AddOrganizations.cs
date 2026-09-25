using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AidSlot.Migrations;

public partial class AddOrganizations : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Organizations",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Organizations", x => x.Id));

        migrationBuilder.AddColumn<int>(name: "OrganizationId", table: "AspNetUsers", type: "integer", nullable: true);
        migrationBuilder.AddColumn<int>(name: "OrganizationId", table: "Campaigns", type: "integer", nullable: true);

        migrationBuilder.CreateIndex(name: "IX_AspNetUsers_OrganizationId", table: "AspNetUsers", column: "OrganizationId");
        migrationBuilder.CreateIndex(name: "IX_Campaigns_OrganizationId", table: "Campaigns", column: "OrganizationId");

        migrationBuilder.AddForeignKey(name: "FK_AspNetUsers_Organizations_OrganizationId", table: "AspNetUsers",
            column: "OrganizationId", principalTable: "Organizations", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey(name: "FK_Campaigns_Organizations_OrganizationId", table: "Campaigns",
            column: "OrganizationId", principalTable: "Organizations", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_AspNetUsers_Organizations_OrganizationId", table: "AspNetUsers");
        migrationBuilder.DropForeignKey(name: "FK_Campaigns_Organizations_OrganizationId", table: "Campaigns");
        migrationBuilder.DropTable(name: "Organizations");
        migrationBuilder.DropIndex(name: "IX_AspNetUsers_OrganizationId", table: "AspNetUsers");
        migrationBuilder.DropIndex(name: "IX_Campaigns_OrganizationId", table: "Campaigns");
        migrationBuilder.DropColumn(name: "OrganizationId", table: "AspNetUsers");
        migrationBuilder.DropColumn(name: "OrganizationId", table: "Campaigns");
    }
}
