using FinancePro.Application.Assets;
using FinancePro.Core.DTOs;
using FinancePro.Core.Enums;
using Xunit;

namespace FinancePro.Application.Tests.Assets;

public sealed class AssetApplicationServiceTests
{
    [Fact]
    public async Task GuardarAsync_DeveFalhar_QuandoDescricaoNaoFoiInformada()
    {
        var service = new AssetApplicationService(new FakeGateway());
        var dto = CriarDtoValido();
        dto.Descricao = string.Empty;

        var result = await service.GuardarAsync(null, dto, 1, "Administrador");

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Contains("descrição", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GuardarAsync_DeveCriarBem_QuandoDadosSaoValidos()
    {
        var gateway = new FakeGateway();
        var service = new AssetApplicationService(gateway);

        var result = await service.GuardarAsync(null, CriarDtoValido(), 1, "Administrador");

        Assert.True(result.IsSuccess);
        Assert.Equal(27, result.Value);
        Assert.True(gateway.CriarChamado);
    }

    [Fact]
    public async Task AbaterAsync_DeveFalhar_QuandoBemInvalido()
    {
        var service = new AssetApplicationService(new FakeGateway());

        var result = await service.AbaterAsync(0, 1, "Administrador");

        Assert.True(result.IsFailure);
    }

    private static NovoBemDto CriarDtoValido() => new()
    {
        EmpresaId = 1,
        NumeroPatrimonial = "PAT-0001",
        Descricao = "Computador portátil",
        DataAquisicao = DateTime.Today,
        ValorAquisicao = 750000,
        VidaUtilAnos = 5,
        MetodoDepreciacao = MetodoDepreciacao.Linear
    };

    private sealed class FakeGateway : IAssetGateway
    {
        public bool CriarChamado { get; private set; }

        public Task<IReadOnlyList<BemListItemDto>> ListarAsync(int empresaId) =>
            Task.FromResult<IReadOnlyList<BemListItemDto>>(Array.Empty<BemListItemDto>());

        public Task<int> CriarAsync(NovoBemDto dto, int utilizadorId, string utilizadorNome)
        {
            CriarChamado = true;
            return Task.FromResult(27);
        }

        public Task AtualizarAsync(int bemId, NovoBemDto dto, int utilizadorId, string utilizadorNome) =>
            Task.CompletedTask;

        public Task AbaterAsync(int bemId, int utilizadorId, string utilizadorNome) =>
            Task.CompletedTask;
    }
}
