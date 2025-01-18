using MediatR;
using RaythaZero.Application.AuthenticationSchemes;
using RaythaZero.Application.Common.Exceptions;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Application.Common.Models;

namespace RaythaZero.Application.Prompts.Queries;

public class GetPromptById
{
    public record Query : GetEntityByIdInputDto, IRequest<IQueryResponseDto<PromptDto>> 
    {
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
            var entity = _db.Prompts.FirstOrDefault(p => p.Id == request.Id.Guid);

            if (entity == null)
                throw new NotFoundException("Prompt", request.Id);

            return new QueryResponseDto<PromptDto>(PromptDto.GetProjection(entity));
        }
    } 
}