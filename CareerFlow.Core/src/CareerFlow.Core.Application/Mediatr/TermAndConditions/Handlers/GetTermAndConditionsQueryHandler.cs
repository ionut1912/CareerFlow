using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Mediatr.TermAndConditions.Query;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Application.Mediator;
using System.Text.Json;

namespace CareerFlow.Core.Application.Mediatr.TermAndConditions.Handlers;

public class GetTermAndConditionsQueryHandler : IRequestHandler<GetTermAndConditionsQuery, TermsAndConditionDto>
{
    private readonly ILogger<GetTermAndConditionsQueryHandler> _logger;
    private readonly ITermsAndConditionsService _termsAndConditionsService;

    public GetTermAndConditionsQueryHandler(ILogger<GetTermAndConditionsQueryHandler> logger, ITermsAndConditionsService termsAndConditionsService)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(termsAndConditionsService, nameof(termsAndConditionsService));
        _logger = logger;
        _termsAndConditionsService = termsAndConditionsService;
    }

    public async Task<TermsAndConditionDto> Handle(GetTermAndConditionsQuery request, CancellationToken cancellationToken = default)
    {
        var termsAndConditions = await _termsAndConditionsService.GetByIdAsync(request.Id, cancellationToken);
        if (termsAndConditions is null)
        {
            _logger.LogWarning("Terms and Conditions with Id {Id} not found.", request.Id);
            throw new TermsAndConditionsNotFoundException($"Terms and Conditions with Id {request.Id} not found.");
        }
        var termsAndConditionsDto = termsAndConditions.ToDto();
        _logger.LogInformation("Terms and Conditions with Id {Id} retrieved successfully,result {termAndConditionsDto}.",
            request.Id, JsonSerializer.Serialize(termsAndConditionsDto, new JsonSerializerOptions { WriteIndented = true }));
        return termsAndConditionsDto;
    }
}
