using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancePro.Data.Migrations;

[Migration("20260820120000_EmpresaSloganCapitalSocial")]
public partial class EmpresaSloganCapitalSocial : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "Slogan", table: "Empresas", type: "nvarchar(200)", maxLength: 200, nullable: true);
        migrationBuilder.AddColumn<decimal>(name: "CapitalSocial", table: "Empresas", type: "decimal(18,2)", nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Slogan", table: "Empresas");
        migrationBuilder.DropColumn(name: "CapitalSocial", table: "Empresas");
    }
}
