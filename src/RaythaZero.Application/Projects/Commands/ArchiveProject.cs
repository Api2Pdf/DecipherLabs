using CSharpVitamins;
using FluentValidation;
using MediatR;
using RaythaZero.Application.Common.Exceptions;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Application.Common.Models;

namespace RaythaZero.Application.Projects.Commands;

public class ArchiveProject
{
    public record Command : LoggableEntityRequest<CommandResponseDto<ShortGuid>>
    {
        public bool IsArchived { get; init; } = true;
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
            
            entity.IsArchived = request.IsArchived;
            
            _db.Projects.Update(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return new CommandResponseDto<ShortGuid>(entity.Id);
        }
    }
}
