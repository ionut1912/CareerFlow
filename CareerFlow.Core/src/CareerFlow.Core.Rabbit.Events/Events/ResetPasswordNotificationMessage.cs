namespace CareerFlow.Core.Rabbit.Events.Events;

public record ResetPasswordNotificationMessage(string Name, string Email, string ResetLink);