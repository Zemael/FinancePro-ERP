using FinancePro.Application.Administration.Users;
using FinancePro.Core.DTOs;
using Xunit;

namespace FinancePro.Application.Tests.Administration;

public sealed class UserAdministrationServiceTests
{
    [Fact]
    public async Task SaveAsync_RejectsInvalidEmail()
    {
        var service = new UserAdministrationService(new FakeGateway());
        var result = await service.SaveAsync(new UserSaveRequest(0, "Utilizador Teste", "email-invalido", 1, 1, true, "123456"));
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task SaveAsync_CreatesValidUser()
    {
        var gateway = new FakeGateway();
        var service = new UserAdministrationService(gateway);
        var result = await service.SaveAsync(new UserSaveRequest(0, "Utilizador Teste", "TESTE@EXEMPLO.COM", 1, 1, true, "123456"));
        Assert.True(result.IsSuccess);
        Assert.Equal("teste@exemplo.com", gateway.LastRequest?.Email);
    }

    private sealed class FakeGateway : IUserAdministrationGateway
    {
        public UserSaveRequest? LastRequest { get; private set; }
        public Task<IReadOnlyList<UtilizadorDto>> ListAsync(string? search, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UtilizadorDto>>([]);
        public Task<int> SaveAsync(UserSaveRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(10);
        }
        public Task SetActiveAsync(int id, bool active, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
