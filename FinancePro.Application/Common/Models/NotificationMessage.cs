namespace FinancePro.Application.Common.Models;

public enum NotificationType
{
    Success,
    Info,
    Warning,
    Error
}

public sealed record NotificationMessage(
    string Message,
    NotificationType Type,
    DateTime CreatedAtUtc);
