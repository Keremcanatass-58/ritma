using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ritma.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessToServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Services",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE Services SET BusinessId = 1 WHERE BusinessId IS NULL");

            migrationBuilder.AlterColumn<int>(
                name: "BusinessId",
                table: "Services",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Services_BusinessId",
                table: "Services",
                column: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Businesses_BusinessId",
                table: "Services",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Businesses_BusinessId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_BusinessId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Services");
        }
    }
}
