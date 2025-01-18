using CSharpVitamins;
using MediatR;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Application.Common.Models;

namespace RaythaZero.Application.Prompts.Commands;

public class DeletePrompt
{
    public record Command : LoggableEntityRequest<CommandResponseDto<ShortGuid>>
    {
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
            var entity = _db.Prompts.First(p => p.Id == request.Id.Guid);

            _db.Prompts.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);

            return new CommandResponseDto<ShortGuid>(request.Id);
        }
    }
}