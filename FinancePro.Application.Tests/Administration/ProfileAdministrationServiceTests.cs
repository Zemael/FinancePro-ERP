using FinancePro.Application.Administration.Profiles;
using FinancePro.Core.DTOs;
using Xunit;

namespace FinancePro.Application.Tests.Administration;

public sealed class ProfileAdministrationServiceTests
{
    [Fact]
    public async Task SaveAsync_RejectsShortName()
    {
        var service = new ProfileAdministrationService(new FakeGateway());
        var result = await service.SaveAsync(new ProfileSaveRequest(0, "TI", null, true));
        Assert.False(result.IsSuccess);
    }

    private sealed class FakeGateway : IProfileAdministrationGateway
    {
        public Task<IReadOnlyList<PerfilDto>> ListAsync(string? search, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<PerfilDto>>([]);
        public Task<int> SaveAsync(ProfileSaveRequest request, CancellationToken cancellationToken = default) => Task.FromResult(1);
        public Task SetActiveAsync(int id, bool active, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
