using FinancePro.Application.Common.Models;

namespace FinancePro.Application.Common.Interfaces;

public interface INotificationService
{
    event EventHandler<NotificationMessage>? NotificationRaised;
    void Success(string message);
    void Info(string message);
    void Warning(string message);
    void Error(string message);
}
