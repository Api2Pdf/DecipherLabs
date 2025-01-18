using CSharpVitamins;
using FluentValidation;
using MediatR;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Application.Common.Models;
using RaythaZero.Application.Common.Utils;
using RaythaZero.Domain.Entities;

namespace RaythaZero.Application.Prompts.Commands;

public class CreatePrompt
{
    public record Command : LoggableRequest<CommandResponseDto<ShortGuid>>
    {
        public string Label { get; init; } = string.Empty;
        public string DeveloperName { get; init; } = string.Empty;
        public string PromptText { get; init; } = string.Empty;
        public string ResultType { get; init; } = string.Empty;
    }
    
    public class Validator : AbstractValidator<CreatePrompt.Command> 
    {
        public Validator(IRaythaDbContext db)
        {
            RuleFor(x => x.Label).NotEmpty();
            RuleFor(x => x.PromptText).NotEmpty();
            RuleFor(x => x.ResultType).NotEmpty();
            RuleFor(x => x.DeveloperName).Must(StringExtensions.IsValidDeveloperName).WithMessage("Invalid developer name.");
            RuleFor(x => x.DeveloperName).Must((request, developerName) =>
            {
                var isDeveloperNameAlreadyExist = db.Prompts.Any(p => p.DeveloperName == request.DeveloperName.ToDeveloperName());
                return !isDeveloperNameAlreadyExist;
            }).WithMessage("A prompt with that developer name already exists.");
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
            var entity = new Prompt
            {
                Id = Guid.NewGuid(),
                Label = request.Label,
                DeveloperName = request.DeveloperName.ToDeveloperName(),
                PromptText = request.PromptText,
                ResultType = request.ResultType
            };

            _db.Prompts.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return new CommandResponseDto<ShortGuid>(entity.Id);
        }
    }
}