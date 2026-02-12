namespace CareerFlow.Core.Rabbit.Events.Events;

public record ResetPasswordNotificationMessage(string Nume, string Email, string ResetLink);