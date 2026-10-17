using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ritma.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessToCustomers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE Customers SET BusinessId = 1 WHERE BusinessId IS NULL");

            migrationBuilder.AlterColumn<int>(
                name: "BusinessId",
                table: "Customers",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_BusinessId",
                table: "Customers",
                column: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Businesses_BusinessId",
                table: "Customers",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Businesses_BusinessId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_BusinessId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Customers");
        }
    }
}
