namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashed);
}