using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Exceptions;
using Shared.Domain.Common;


namespace CareerFlow.Core.Domain.Entities
{
    public class Account : Entity
    {
        private Account() //for EF core
        {
        }

        private Account(string email, string password, string username,string name)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException("Email cannot be null or empty");
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException("Password cannot be null or empty");
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException("Username cannot be null or empty");

            if(string.IsNullOrEmpty(name))
                throw new ArgumentNullException("Name cannot be null or empty");

            Email = email;
            Password = password;
            Username = username;
            Name=name;
            IsFounder = false;
            CreatedAt = DateTime.UtcNow;
        }

        public string Email { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;
        public string Username { get; private set; } = string.Empty;
        public string  Name{get;private set;}=string.Empty;
        public bool IsFounder { get; private set; } = false;
        public bool TermsAccepted { get; private set; } = false;
        public bool PrivacyPolicyAccepted { get; private set; } = false;

        public static Account Create(string email, string password, string username,string name)
        {
            return new Account(email, password, username,name);
        }

        public void HashPassword(IPasswordService passwordService)
        {
            if (string.IsNullOrWhiteSpace(Password))
                throw new PasswordNotEmptyException("Password cannot be empty before hashing.");

            Password = passwordService.HashPassword(Password);
        }

        public void ResetPassword(string newPassword, IPasswordService passwordService)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new PasswordNotEmptyException("New password cannot be empty.");
            Password = passwordService.HashPassword(newPassword);
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsFounder()
        {
            IsFounder = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateTerns()
        {
            TermsAccepted = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AcceptTerms()
        {
            TermsAccepted = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePrivacyPolicy()
        {
            PrivacyPolicyAccepted = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AcceptPrivacyPolicy()
        {
            PrivacyPolicyAccepted = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
