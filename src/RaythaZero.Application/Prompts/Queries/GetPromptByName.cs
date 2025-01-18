using MediatR;
using RaythaZero.Application.Common.Exceptions;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Application.Common.Models;
using RaythaZero.Application.Common.Utils;

namespace RaythaZero.Application.Prompts.Queries;

public class GetPromptByName
{
    public record Query : IRequest<IQueryResponseDto<PromptDto>>
    {
        public string DeveloperName { get; init; } = string.Empty;
    }

    public class Handler : IRequestHandler<Query, IQueryResponseDto<PromptDto>>
    {
        private readonly IRaythaDbContext _db;
        public Handler(IRaythaDbContext db)
        {
            _db = db;
        }
        public async Task<IQueryResponseDto<PromptDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var entity = _db.Prompts.FirstOrDefault(p => p.DeveloperName == request.DeveloperName.ToDeveloperName());

            if (entity == null)
                throw new NotFoundException("Prompt", request.DeveloperName);

            return new QueryResponseDto<PromptDto>(PromptDto.GetProjection(entity));
        }
    } 
}