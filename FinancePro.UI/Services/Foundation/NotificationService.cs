using FinancePro.Application.Common.Interfaces;
using FinancePro.Application.Common.Models;

namespace FinancePro.UI.Services.Foundation;

public sealed class NotificationService : INotificationService
{
    public event EventHandler<NotificationMessage>? NotificationRaised;

    public void Success(string message) => Raise(message, NotificationType.Success);
    public void Info(string message) => Raise(message, NotificationType.Info);
    public void Warning(string message) => Raise(message, NotificationType.Warning);
    public void Error(string message) => Raise(message, NotificationType.Error);

    private void Raise(string message, NotificationType type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        NotificationRaised?.Invoke(this, new NotificationMessage(message, type, DateTime.UtcNow));
    }
}
