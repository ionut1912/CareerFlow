using CareerFlow.Core.Domain.Exceptions;
using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.Entities
{
    public class RefreshToken : Entity
    {
        public Guid UserId { get; private set; }
        public string Token { get; private set; } = string.Empty;
        public string JwtToken { get; private set; } = string.Empty;
        public bool IsUsed { get; private set; }
        public bool IsRevoked { get; private set; }
        public DateTime ExpiryDate { get; private set; }

        private RefreshToken()
        {
        }

        private RefreshToken(Guid userId, string token, string jwtToken, DateTime expiryDate)
        {
            if (userId == Guid.Empty)
                throw new InvalidFieldException("User id-ul este invalid");
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidFieldException("Tokenul este invalid");
            if (string.IsNullOrWhiteSpace(jwtToken))
                throw new InvalidFieldException("Jwt-ul este invalid");

            if (expiryDate == default)
                throw new InvalidFieldException("Data de expirare este invalida");
            if (expiryDate <= DateTime.UtcNow)
                throw new InvalidFieldException("Data de expirare este in trecut");

            UserId = userId;
            Token = token;
            JwtToken = jwtToken;
            IsUsed = false;
            IsRevoked = false;
            CreatedAt = DateTime.UtcNow;
            ExpiryDate = expiryDate;
        }

        public static RefreshToken Create(Guid userId, string token, string jwtToken, DateTime expiryDate)
        {
            return new RefreshToken(userId, token, jwtToken, expiryDate);
        }

        public void MarkAsUsed()
        {
            IsUsed = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsRevoked()
        {
            IsRevoked = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
