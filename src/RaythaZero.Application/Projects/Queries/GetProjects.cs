using MediatR;
using Microsoft.EntityFrameworkCore;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Application.Common.Models;
using RaythaZero.Application.Common.Utils;
using RaythaZero.Domain.ValueObjects;

namespace RaythaZero.Application.Projects.Queries;

public class GetProjects
{
    public record Query : GetPagedEntitiesInputDto, IRequest<IQueryResponseDto<ListResultDto<ProjectDto>>> 
    { 
        public override string OrderBy { get; init; } = $"CreationTime {SortOrder.DESCENDING}";
        public bool ShowArchived { get; init; } = false;
    }

    public class Handler : IRequestHandler<Query, IQueryResponseDto<ListResultDto<ProjectDto>>>
    {
        private readonly IRaythaDbContext _db;
        public Handler(IRaythaDbContext db)
        {
            _db = db;
        }

        public async Task<IQueryResponseDto<ListResultDto<ProjectDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _db.Projects.AsQueryable().Where(p => p.IsArchived == request.ShowArchived);
               
            if (!string.IsNullOrEmpty(request.Search))
            {
                var searchQuery = request.Search.ToLower();
                query = query
                    .Where(d =>
                        d.Label.ToLower().Contains(searchQuery));
            }

            var total = await query.CountAsync();
            var items = query.ApplyPaginationInput(request).Select(ProjectDto.GetProjection()).ToArray();

            return new QueryResponseDto<ListResultDto<ProjectDto>>(new ListResultDto<ProjectDto>(items, total));
        }
    }
}