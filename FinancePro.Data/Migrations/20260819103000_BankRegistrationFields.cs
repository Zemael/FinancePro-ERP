using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancePro.Data.Migrations;

[Migration("20260819103000_BankRegistrationFields")]
public partial class BankRegistrationFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "Sigla", table: "Bancos", type: "nvarchar(20)", maxLength: 20, nullable: true);
        migrationBuilder.AddColumn<string>(name: "Endereco", table: "Bancos", type: "nvarchar(250)", maxLength: 250, nullable: true);
        migrationBuilder.AddColumn<string>(name: "Contacto", table: "Bancos", type: "nvarchar(50)", maxLength: 50, nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Sigla", table: "Bancos");
        migrationBuilder.DropColumn(name: "Endereco", table: "Bancos");
        migrationBuilder.DropColumn(name: "Contacto", table: "Bancos");
    }
}
