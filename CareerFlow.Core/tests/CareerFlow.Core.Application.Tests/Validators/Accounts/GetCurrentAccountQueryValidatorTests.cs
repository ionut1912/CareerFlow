using CareerFlow.Core.Application.CQRS.Accounts.Query;
using CareerFlow.Core.Application.Validators.Account;
using FluentValidation.TestHelper;

namespace CareerFlow.Core.Application.Tests.Validators.Accounts;

public class GetCurrentAccountQueryValidatorTests
{
    private readonly GetCurrentAccountQueryValidator _validator;

    public GetCurrentAccountQueryValidatorTests()
    {
        _validator = new GetCurrentAccountQueryValidator();
    }

    [Fact]
    public void Validate_WhenQueryIsValid_ShouldNotHaveErrors()
    {
        //Arrange
        var query = new GetCurrentAccountQuery(Guid.NewGuid());

        //Act
        var result = _validator.TestValidate(query);

        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }


    [Theory]
    [MemberData(nameof(GetInvalidGuids))]
    public void Validate_WhenAccountIdIsEmpty_ShouldHaveErrors(Guid accountId)
    {
        //Arrange
        var query = new GetCurrentAccountQuery(accountId);

        //Act
        var result = _validator.TestValidate(query);

        //Assert
        result.ShouldHaveValidationErrorFor(x => x.AccountId)
            .WithErrorMessage("Id-ul contului este necesar");
    }


    public static IEnumerable<object[]> GetInvalidGuids()
    {
        yield return new object[] { Guid.Empty };
    }
}