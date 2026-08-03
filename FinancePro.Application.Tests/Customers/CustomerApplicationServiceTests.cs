using FinancePro.Application.Customers;
using FinancePro.Core.DTOs;
using Xunit;

namespace FinancePro.Application.Tests.Customers;

public sealed class CustomerApplicationServiceTests
{
    [Fact] public async Task Save_rejects_empty_name()
    {
        var sut=new CustomerApplicationService(new FakeGateway());
        var result=await sut.SaveAsync(new CustomerSaveRequest(0,1,"",null,null,null,null));
        Assert.True(result.IsFailure);
    }
    private sealed class FakeGateway : ICustomerGateway
    {
        public Task<bool> ExistsWithNifAsync(int empresaId,string nif,int ignoreId,CancellationToken c=default)=>Task.FromResult(false);
        public Task<IReadOnlyList<ClienteDto>> ListAsync(int empresaId,string? pesquisa,CancellationToken c=default)=>Task.FromResult<IReadOnlyList<ClienteDto>>(Array.Empty<ClienteDto>());
        public Task<int> SaveAsync(CustomerSaveRequest request,CancellationToken c=default)=>Task.FromResult(1);
        public Task SetActiveAsync(int id,bool ativo,CancellationToken c=default)=>Task.CompletedTask;
    }
}
