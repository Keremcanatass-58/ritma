using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ritma.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessToAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE Appointments SET BusinessId = 1 WHERE BusinessId IS NULL");

            migrationBuilder.AlterColumn<int>(
                name: "BusinessId",
                table: "Appointments",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_BusinessId",
                table: "Appointments",
                column: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Businesses_BusinessId",
                table: "Appointments",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Businesses_BusinessId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_BusinessId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Appointments");
        }
    }
}
