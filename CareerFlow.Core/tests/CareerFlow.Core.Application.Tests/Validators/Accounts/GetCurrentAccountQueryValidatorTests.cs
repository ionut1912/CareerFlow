using CareerFlow.Core.Application.CQRS.Accounts.Queries;
using CareerFlow.Core.Application.Validators.Account;
using FluentValidation.TestHelper;

namespace CareerFlow.Core.Application.Tests.Validators.Accounts;

public class GetCurrentAccountQueryValidatorTests
{
    private readonly GetCurrentAccountQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveErrors()
    {
        //Arrange
        var query = new GetCurrentAccountQuery(Guid.NewGuid());

        //Act
        TestValidationResult<GetCurrentAccountQuery>? result = _validator.TestValidate(query);

        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }


    [Theory]
    [MemberData(nameof(GetInvalidGuids))]
    public void Validate_WhenAccountIdIsEmpty_ShouldHaveErrors(string accountId)
    {
        //Arrange
        var query = new GetCurrentAccountQuery(Guid.Parse(accountId));

        //Act
        TestValidationResult<GetCurrentAccountQuery>? result = _validator.TestValidate(query);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.AccountId)
            .WithErrorMessage("Id-ul contului este necesar");
    }


    public static TheoryData<string> GetInvalidGuids() => [Guid.Empty.ToString()];
}
