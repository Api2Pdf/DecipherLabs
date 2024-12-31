using CSharpVitamins;
using FluentValidation;
using MediatR;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Application.Common.Models;
using RaythaZero.Application.Common.Utils;
using RaythaZero.Domain.Entities;

namespace RaythaZero.Application.Roles.Commands;

public class CreateRole
{
    public record Command : LoggableRequest<CommandResponseDto<ShortGuid>>
    {
        public string Label { get; init; } = null!;
        public string DeveloperName { get; init; } = null!;
        public IEnumerable<string> SystemPermissions { get; init; } = null!;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(IRaythaDbContext db)
        {
            RuleFor(x => x.Label).NotEmpty();
            RuleFor(x => x.DeveloperName).Must(StringExtensions.IsValidDeveloperName).WithMessage("Invalid developer name.");
            RuleFor(x => x.DeveloperName).Must((request, developerName) =>
            {
                var entity = db.Roles.FirstOrDefault(p => p.DeveloperName == request.DeveloperName.ToDeveloperName());
                return !(entity != null);
            }).WithMessage("A role with that developer name already exists.");
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
            var builtInSystemPermissions = BuiltInSystemPermission.From(request.SystemPermissions.ToArray());
            
            Role entity = new Role
            {
                Label = request.Label,
                DeveloperName = request.DeveloperName.ToDeveloperName(),
                SystemPermissions = builtInSystemPermissions
            };

            _db.Roles.Add(entity);

            await _db.SaveChangesAsync(cancellationToken);
            return new CommandResponseDto<ShortGuid>(entity.Id);
        }
    }
}
