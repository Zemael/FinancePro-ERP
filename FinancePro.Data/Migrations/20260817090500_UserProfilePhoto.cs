using FinancePro.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancePro.Data.Migrations;

[DbContext(typeof(FinanceProDbContext))]
[Migration("20260817090500_UserProfilePhoto")]
public partial class UserProfilePhoto : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<byte[]>(
            name: "FotoPerfil",
            table: "Utilizadores",
            type: "varbinary(max)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "FotoPerfil", table: "Utilizadores");
    }
}
