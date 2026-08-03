using FinancePro.Application.Suppliers;
using FinancePro.Core.DTOs;
using Xunit;

namespace FinancePro.Application.Tests.Suppliers;

public sealed class SupplierApplicationServiceTests
{
    [Fact] public async Task Save_rejects_invalid_email()
    {
        var sut=new SupplierApplicationService(new FakeGateway());
        var result=await sut.SaveAsync(new SupplierSaveRequest(0,1,"Fornecedor",null,null,"email-invalido",null));
        Assert.True(result.IsFailure);
    }
    private sealed class FakeGateway : ISupplierGateway
    {
        public Task<bool> ExistsWithNifAsync(int empresaId,string nif,int ignoreId,CancellationToken c=default)=>Task.FromResult(false);
        public Task<IReadOnlyList<FornecedorDto>> ListAsync(int empresaId,string? pesquisa,CancellationToken c=default)=>Task.FromResult<IReadOnlyList<FornecedorDto>>(Array.Empty<FornecedorDto>());
        public Task<int> SaveAsync(SupplierSaveRequest request,CancellationToken c=default)=>Task.FromResult(1);
        public Task SetActiveAsync(int id,bool ativo,CancellationToken c=default)=>Task.CompletedTask;
    }
}
