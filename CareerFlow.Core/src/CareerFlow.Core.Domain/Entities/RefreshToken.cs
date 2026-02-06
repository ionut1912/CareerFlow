using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.Entities
{
    public class RefreshToken:Entity
    {
        public Guid UserId { get; private set; }
        public string Token { get; private set; }=string.Empty;
        public string JwtId { get; private  set; } =string.Empty;
        public bool IsUsed { get; private set; }
        public bool IsRevoked { get; private set; }
        public DateTime ExpiryDate { get; private set; }

        private RefreshToken()
        {
        }

        private RefreshToken(Guid userId, string token, string jwtId, DateTime expiryDate)
        {
            UserId = userId;
            Token = token;
            JwtId = jwtId;
            IsUsed = false;
            IsRevoked = false;
            CreatedAt = DateTime.UtcNow;
            ExpiryDate = expiryDate;
        }

        public static RefreshToken Create(Guid userId, string token, string jwtId, DateTime expiryDate)
        {
            return new RefreshToken(userId, token, jwtId, expiryDate);
        }

        public void MarkAsUsed()
        {
            IsUsed = true;
            UpdatedAt = DateTime.UtcNow;
        }   
    }
}
