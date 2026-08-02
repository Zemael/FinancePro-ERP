namespace FinancePro.Application.Common.Interfaces;

public interface IDialogService
{
    Task<bool> ConfirmAsync(string message, string title = "Confirmação");
    Task ShowInfoAsync(string message, string title = "FinancePro");
    Task ShowWarningAsync(string message, string title = "Aviso");
    Task ShowErrorAsync(string message, string title = "Erro");
}
