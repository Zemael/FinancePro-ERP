using FinancePro.Application.Common.Interfaces;

namespace FinancePro.UI.Services.Foundation;

public sealed class NavigationService : INavigationService
{
    public event EventHandler<NavigationRequestedEventArgs>? NavigationRequested;

    public void Navigate(string destination, object? parameter = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        NavigationRequested?.Invoke(this, new NavigationRequestedEventArgs(destination, parameter));
    }
}
