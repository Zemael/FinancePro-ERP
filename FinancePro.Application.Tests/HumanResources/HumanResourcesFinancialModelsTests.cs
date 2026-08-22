using FinancePro.Platform.HumanResources;
using Xunit;

namespace FinancePro.Application.Tests.HumanResources;

public sealed class HumanResourcesFinancialModelsTests
{
    [Fact]
    public void Colaborador_DeveCalcularLiquidoMensal()
    {
        var row = Employee(250_000m, 40_000m, 25_000m);
        Assert.Equal(265_000m, row.NetMonthly);
    }

    [Fact]
    public void Colaborador_SemAbonosNemDescontos_DeveManterSalarioBase()
    {
        var row = Employee(180_000m, 0m, 0m);
        Assert.Equal(180_000m, row.NetMonthly);
    }

    [Fact]
    public void Colaborador_ComDescontoIntegral_DeveTerLiquidoZero()
    {
        var row = Employee(100_000m, 20_000m, 120_000m);
        Assert.Equal(0m, row.NetMonthly);
    }

    [Fact]
    public void Folha_DeveFormatarPeriodoComDoisDigitos()
    {
        var row = new PayrollRunRow(1, 2026, 3, "Rascunho", DateTime.UtcNow, null, "", 2, 0, 0, 0);
        Assert.Equal("03/2026", row.Period);
    }

    [Fact]
    public void Folha_DeDezembro_DeveFormatarPeriodoCorretamente()
    {
        var row = new PayrollRunRow(1, 2026, 12, "Aprovada", DateTime.UtcNow, DateTime.UtcNow, "", 2, 0, 0, 0);
        Assert.Equal("12/2026", row.Period);
    }

    [Fact]
    public void Ferias_DeveSomarDireitoESaldoTransitado()
    {
        var row = Leave(22m, 5m, 0m);
        Assert.Equal(27m, row.AvailableDays);
    }

    [Fact]
    public void Ferias_DeveDeduzirDiasUtilizados()
    {
        var row = Leave(22m, 3m, 8m);
        Assert.Equal(17m, row.AvailableDays);
    }

    [Fact]
    public void Ferias_PodemApresentarSaldoNegativo()
    {
        var row = Leave(22m, 0m, 25m);
        Assert.Equal(-3m, row.AvailableDays);
    }

    [Fact]
    public void Emprestimo_DevePreservarSaldoEPrestacao()
    {
        var row = new EmployeeLoanRow(1, 2, "Empréstimo", "EMP-1", DateTime.Today, 120_000m, 80_000m, 20_000m, DateTime.Today.AddMonths(1), "Ativo", "");
        Assert.Equal(80_000m, row.OutstandingBalance);
        Assert.Equal(20_000m, row.InstallmentAmount);
    }

    [Fact]
    public void RegraSalarial_DevePreservarTaxaELimites()
    {
        var row = new PayrollRuleRow(1, "Imposto salarial", "Escalão A", 10m, 500m, 100_000m, 300_000m, DateTime.Today, null, "");
        Assert.Equal(10m, row.Rate);
        Assert.Equal(100_000m, row.MinimumBase);
        Assert.Equal(300_000m, row.MaximumBase!.Value);
    }

    private static EmployeeFinancialRow Employee(decimal salary, decimal allowances, decimal deductions) =>
        new(1, "COL-001", "Colaborador", "", "", "Administração", "", "Técnico", "Efetivo",
            DateTime.Today, null, salary, allowances, deductions, "", "", "Ativo", "");

    private static EmployeeLeaveBalanceRow Leave(decimal entitled, decimal carried, decimal taken) =>
        new(1, 1, 2026, entitled, carried, taken, "");
}
