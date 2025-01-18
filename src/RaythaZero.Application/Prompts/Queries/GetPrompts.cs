using MediatR;
using Microsoft.EntityFrameworkCore;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Application.Common.Models;
using RaythaZero.Application.Common.Utils;
using RaythaZero.Domain.ValueObjects;

namespace RaythaZero.Application.Prompts.Queries;

public class GetPrompts
{
    public record Query : GetPagedEntitiesInputDto, IRequest<IQueryResponseDto<ListResultDto<PromptDto>>> 
    { 
        public override string OrderBy { get; init; } = $"Label {SortOrder.Ascending}";
    }

    public class Handler : IRequestHandler<Query, IQueryResponseDto<ListResultDto<PromptDto>>>
    {
        private readonly IRaythaDbContext _db;
        public Handler(IRaythaDbContext db)
        {
            _db = db;
        }

        public async Task<IQueryResponseDto<ListResultDto<PromptDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _db.Prompts
                .Include(p => p.LastModifierUser)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.Search))
            {
                var searchQuery = request.Search.ToLower();
                query = query
                    .Where(d =>
                        d.Label.ToLower().Contains(searchQuery) ||
                        d.DeveloperName.ToLower().Contains(searchQuery));           
            }

            var total = await query.CountAsync();
            var items = query.ApplyPaginationInput(request).Select(PromptDto.GetProjection()).ToArray();

            return new QueryResponseDto<ListResultDto<PromptDto>>(new ListResultDto<PromptDto>(items, total));
        }
    } 
}