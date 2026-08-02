namespace FinancePro.Application.Common.Interfaces;

public interface INavigationService
{
    event EventHandler<NavigationRequestedEventArgs>? NavigationRequested;
    void Navigate(string destination, object? parameter = null);
}

public sealed record NavigationRequestedEventArgs(string Destination, object? Parameter);
