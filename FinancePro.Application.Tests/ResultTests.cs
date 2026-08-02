using Xunit;
using FinancePro.Application.Common.Results;

namespace FinancePro.Application.Tests;

public sealed class ResultTests
{
    [Fact]
    public void Success_SemValor_DeveRepresentarSucesso()
    {
        var result = Result.Success("Operação concluída.");

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Empty(result.Errors);
        Assert.Equal("Operação concluída.", result.Message);
    }

    [Fact]
    public void Failure_DeveRemoverErrosVaziosEDuplicados()
    {
        var result = Result.Failure(new[] { "Erro A", "", "Erro A", "Erro B" });

        Assert.True(result.IsFailure);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains("Erro A", result.Errors);
        Assert.Contains("Erro B", result.Errors);
    }

    [Fact]
    public void Success_ComValor_DeveDisponibilizarValor()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Failure_ComValor_NaoDeveDisponibilizarValor()
    {
        var result = Result<int>.Failure("Falha de validação.");

        Assert.True(result.IsFailure);
        Assert.Equal(default, result.Value);
        Assert.Single(result.Errors);
    }
}
