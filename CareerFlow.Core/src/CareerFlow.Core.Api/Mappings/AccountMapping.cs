using CareerFlow.Core.Api.Requests;
using CareerFlow.Core.Application.Mediatr.Accounts.Commands;
using CareerFlow.Core.Application.Mediatr.Accounts.Query;

namespace CareerFlow.Core.Api.Mappings
{
    public static class AccountMapping
    {
        public static CreateAccountCommand ToCreateCommand(this CreateAccountRequest request)
        {
            return new CreateAccountCommand(request.Email, request.Password, request.Username);
        }

        public static LoginQuery ToLoginQuery(this LoginRequest request)
        {
            return new LoginQuery(request.Username, request.Password);
        }

        public static ResetPasswordCommand ToResetPasswordCommand(this ResetPasswordRequest request, string username)
        {
            return new ResetPasswordCommand(username, request.NewPassword);
        }
    }
}
