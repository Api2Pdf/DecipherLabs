using CSharpVitamins;
using FluentValidation;
using MediatR;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Application.Common.Models;
using RaythaZero.Application.Common.Utils;
using RaythaZero.Domain.Entities;

namespace RaythaZero.Application.OrganizationSettings.Commands;

public class EditCompanylevelInfo
{
    public record Command : LoggableRequest<CommandResponseDto<ShortGuid>>
    {
        public string LegalName { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
        public string CityHq { get; init; } = string.Empty;
        public string StateHq { get; init; } = string.Empty;
        public bool OffersBenefits { get; init; } = false;
        public string OffersBenefitsDescription { get; init; } = string.Empty;
        public string WageRateSheetMediaId { get; init; }
        public string PreviousCostVolumeExcelMediaId { get; init; }
        public string PreviousCostVolumeWordMediaId { get; init; }
        public string BalanceSheetMediaId { get; init; }
        public string ProfitAndLossMediaId { get; init; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.LegalName).NotEmpty();
            RuleFor(x => x.Url).NotEmpty();
            RuleFor(x => x.CityHq).NotEmpty();
            RuleFor(x => x.StateHq).NotEmpty();
        }
    }

    public class Handler : IRequestHandler<Command, CommandResponseDto<ShortGuid>>
    {
        private readonly IRaythaDbContext _db;
        public Handler(IRaythaDbContext db)
        {
            _db = db;
        }
        public async Task<CommandResponseDto<ShortGuid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var entity = _db.OrganizationSettings.First();
            var companyInfo = new CompanyLevelInfo()
            {
                LegalName = request.LegalName,
                Url = request.Url,
                CityHq = request.CityHq,
                StateHq = request.StateHq,
                OffersBenefits = request.OffersBenefits,
                OffersBenefitsDescription = request.OffersBenefitsDescription,
                WageRateSheetMediaId = request.WageRateSheetMediaId,
                PreviousCostVolumeExcelMediaId = request.PreviousCostVolumeExcelMediaId,
                PreviousCostVolumeWordMediaId = request.PreviousCostVolumeWordMediaId,
                BalanceSheetMediaId = request.BalanceSheetMediaId,
                ProfitAndLossMediaId = request.ProfitAndLossMediaId
            };
            entity.CompanySetupData = companyInfo;
            await _db.SaveChangesAsync(cancellationToken);
            return new CommandResponseDto<ShortGuid>(entity.Id);
        }
    }
}
