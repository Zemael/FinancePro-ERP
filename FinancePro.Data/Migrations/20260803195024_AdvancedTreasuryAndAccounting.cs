using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancePro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdvancedTreasuryAndAccounting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AceitaLancamentos",
                table: "PlanoContas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CentroCustoObrigatorio",
                table: "PlanoContas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Natureza",
                table: "PlanoContas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "LancamentosContabeis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    DataLancamento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    DocumentoReferencia = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    OrigemModulo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DataContabilizacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UtilizadorId = table.Column<int>(type: "int", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LancamentosContabeis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LancamentosContabeis_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LancamentosContabeis_Utilizadores_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "Utilizadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LancamentoContabilLinhas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LancamentoContabilId = table.Column<int>(type: "int", nullable: false),
                    PlanoContasId = table.Column<int>(type: "int", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Debito = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Credito = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CentroCusto = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LancamentoContabilLinhas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LancamentoContabilLinhas_LancamentosContabeis_LancamentoContabilId",
                        column: x => x.LancamentoContabilId,
                        principalTable: "LancamentosContabeis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LancamentoContabilLinhas_PlanoContas_PlanoContasId",
                        column: x => x.PlanoContasId,
                        principalTable: "PlanoContas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LancamentoContabilLinhas_LancamentoContabilId",
                table: "LancamentoContabilLinhas",
                column: "LancamentoContabilId");

            migrationBuilder.CreateIndex(
                name: "IX_LancamentoContabilLinhas_PlanoContasId",
                table: "LancamentoContabilLinhas",
                column: "PlanoContasId");

            migrationBuilder.CreateIndex(
                name: "IX_LancamentosContabeis_EmpresaId_Numero",
                table: "LancamentosContabeis",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LancamentosContabeis_UtilizadorId",
                table: "LancamentosContabeis",
                column: "UtilizadorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LancamentoContabilLinhas");

            migrationBuilder.DropTable(
                name: "LancamentosContabeis");

            migrationBuilder.DropColumn(
                name: "AceitaLancamentos",
                table: "PlanoContas");

            migrationBuilder.DropColumn(
                name: "CentroCustoObrigatorio",
                table: "PlanoContas");

            migrationBuilder.DropColumn(
                name: "Natureza",
                table: "PlanoContas");
        }
    }
}
