using CSharpVitamins;
using FluentValidation;
using MediatR;
using RaythaZero.Application.Common.Exceptions;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Application.Common.Models;

namespace RaythaZero.Application.Prompts.Commands;

public class EditPrompt
{
    public record Command : LoggableEntityRequest<CommandResponseDto<ShortGuid>>
    {
        public string Label { get; init; } = string.Empty;
        public string PromptText { get; init; } = string.Empty;
        public string ResultType { get; init; } = string.Empty;
    }
    
    public class Validator : AbstractValidator<Command> 
    {
        public Validator(IRaythaDbContext db)
        {
            RuleFor(x => x.Label).NotEmpty();
            RuleFor(x => x.PromptText).NotEmpty();
            RuleFor(x => x.ResultType).NotEmpty();
            RuleFor(x => x).Custom((request, context) =>
            {
                var entity = db.Prompts.FirstOrDefault(p => p.Id == request.Id.Guid);
                if (entity == null)
                    throw new NotFoundException("Prompt", request.Id);
            });
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
            var entity = _db.Prompts.First(p => p.Id == request.Id.Guid);
            entity.Label = request.Label;
            entity.PromptText = request.PromptText;
            entity.ResultType = request.ResultType;

            _db.Prompts.Update(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return new CommandResponseDto<ShortGuid>(entity.Id);
        }
    }
}