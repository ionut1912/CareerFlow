using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Exceptions;
using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.Entities;

public class Account : Entity
{
    private Account() //for EF core
    {
    }

    private Account(string email, string password, string username, string name)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidFieldException("Email-ul este invalid");
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidFieldException("Parola este ivalida");
        if (string.IsNullOrWhiteSpace(username))
            throw new InvalidFieldException("Username-ul este invalid");

        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidFieldException("Numele este invalid");

        Email = email;
        Password = password;
        Username = username;
        Name = name;
        IsFounder = false;
        PrivacyPolicyAccepted = false;
        TermsAccepted = false;
        CreatedAt = DateTime.UtcNow;
    }

    public string Email { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsFounder { get; private set; }
    public bool TermsAccepted { get; private set; }
    public bool PrivacyPolicyAccepted { get; private set; }

    public static Account Create(string email, string password, string username, string name)
    {
        return new Account(email, password, username, name);
    }

    public void HashPassword(IPasswordService passwordService)
    {
        Password = passwordService.HashPassword(Password);
    }

    public void ResetPassword(string newPassword, IPasswordService passwordService)
    {
        Password = passwordService.HashPassword(newPassword);
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFounder()
    {
        IsFounder = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AcceptTerms()
    {
        TermsAccepted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AcceptPrivacyPolicy()
    {
        PrivacyPolicyAccepted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}