using Ritma.Api.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ritma.Api.Migrations;

[DbContext(typeof(RitmaDbContext))]
[Migration("20261018173000_AddBookingSettings")]
public partial class AddBookingSettings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "BookingEnabled",
            table: "Businesses",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<TimeOnly>(
            name: "OpeningTime",
            table: "Businesses",
            type: "time",
            nullable: false,
            defaultValue: new TimeOnly(9, 0));

        migrationBuilder.AddColumn<TimeOnly>(
            name: "ClosingTime",
            table: "Businesses",
            type: "time",
            nullable: false,
            defaultValue: new TimeOnly(18, 0));

        migrationBuilder.AddColumn<byte>(
            name: "WorkingDaysMask",
            table: "Businesses",
            type: "tinyint",
            nullable: false,
            defaultValue: (byte)126);

        migrationBuilder.AddColumn<string>(
            name: "PublicManageTokenHash",
            table: "Appointments",
            type: "nvarchar(64)",
            maxLength: 64,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Appointments_PublicManageTokenHash",
            table: "Appointments",
            column: "PublicManageTokenHash",
            unique: true,
            filter: "[PublicManageTokenHash] IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Appointments_PublicManageTokenHash",
            table: "Appointments");

        migrationBuilder.DropColumn(name: "PublicManageTokenHash", table: "Appointments");
        migrationBuilder.DropColumn(name: "BookingEnabled", table: "Businesses");
        migrationBuilder.DropColumn(name: "OpeningTime", table: "Businesses");
        migrationBuilder.DropColumn(name: "ClosingTime", table: "Businesses");
        migrationBuilder.DropColumn(name: "WorkingDaysMask", table: "Businesses");
    }
}
