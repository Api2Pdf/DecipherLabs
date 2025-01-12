using CSharpVitamins;
using FluentValidation;
using MediatR;
using RaythaZero.Application.Common.Exceptions;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Application.Common.Models;

namespace RaythaZero.Application.Projects.Commands;

public class EditProject
{
    public record Command : LoggableEntityRequest<CommandResponseDto<ShortGuid>>
    {
        public string Label { get; init; } = null!;
        public string DsipProposalNumber { get; init; } = string.Empty;
        public string TypeOfProposal { get; init; } = string.Empty;
        public string TopLevelMediaId { get; init; } = string.Empty;
        public string ServiceSpecificMediaId { get; init; } = string.Empty;
        public string TopicMediaId { get; init; } = string.Empty;
        public IEnumerable<string> OtherDirectCostSelections { get; init; } = new List<string>();
    }

    public class Validator : AbstractValidator<Command> 
    {
        public Validator(IRaythaDbContext db)
        {
            RuleFor(x => x.Label).NotEmpty();
            RuleFor(x => x.TypeOfProposal).NotEmpty();
            RuleFor(x => x.DsipProposalNumber).NotEmpty();
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
            var entity = _db.Projects
                .FirstOrDefault(p => p.Id == request.Id.Guid);

            if (entity == null)
                throw new NotFoundException("Project", request.Id);
            
            entity.Label = request.Label;
            var projectData = entity.ProjectData;
            projectData.TypeOfProposal = request.TypeOfProposal;
            projectData.DsipProposalNumber = request.DsipProposalNumber;
            projectData.TopLevelMediaId = request.TopLevelMediaId;
            projectData.ServiceSpecificMediaId = request.ServiceSpecificMediaId;
            projectData.TopicMediaId = request.TopicMediaId;
            projectData.OtherDirectCostSelections = request.OtherDirectCostSelections;
            entity.ProjectData = projectData;
            
            _db.Projects.Update(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return new CommandResponseDto<ShortGuid>(entity.Id);
        }
    }
}
