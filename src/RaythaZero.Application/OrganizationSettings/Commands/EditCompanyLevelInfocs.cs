using CSharpVitamins;
using FluentValidation;
using MediatR;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Application.Common.Models;
using RaythaZero.Application.Common.Utils;

namespace RaythaZero.Application.OrganizationSettings.Commands;

public class EditCompanylevelInfo
{
    public record Command : LoggableRequest<CommandResponseDto<ShortGuid>>
    {
        public string LegalName { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
        public string CityAndStateHq { get; init; } = string.Empty;
        public bool OffersBenefits { get; init; } = false;
        public string OffersBenefitsDescription { get; init; } = string.Empty;
        public ShortGuid? WageRateSheetMediaId { get; init; }
        public ShortGuid? PreviousCostVolumeExcelMediaId { get; init; }
        public ShortGuid? PreviousCostVolumeWordMediaId { get; init; }
        public IEnumerable<ShortGuid> FinancialStatements { get; init; } =  new List<ShortGuid>();
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.LegalName).NotEmpty();
            RuleFor(x => x.Url).NotEmpty();
            RuleFor(x => x.CityAndStateHq).NotEmpty();
            RuleFor(x => x.WageRateSheetMediaId).NotEmpty();
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
            entity.CompanySetupData = request;
            await _db.SaveChangesAsync(cancellationToken);
            return new CommandResponseDto<ShortGuid>(entity.Id);
        }
    }
}
