using System.Windows;
using FinancePro.Application.Common.Interfaces;

namespace FinancePro.UI.Services.Foundation;

public sealed class DialogService : IDialogService
{
    public Task<bool> ConfirmAsync(string message, string title = "Confirmação") =>
        Task.FromResult(MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes);

    public Task ShowInfoAsync(string message, string title = "FinancePro") =>
        ShowAsync(message, title, MessageBoxImage.Information);

    public Task ShowWarningAsync(string message, string title = "Aviso") =>
        ShowAsync(message, title, MessageBoxImage.Warning);

    public Task ShowErrorAsync(string message, string title = "Erro") =>
        ShowAsync(message, title, MessageBoxImage.Error);

    private static Task ShowAsync(string message, string title, MessageBoxImage icon)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        return Task.CompletedTask;
    }
}
