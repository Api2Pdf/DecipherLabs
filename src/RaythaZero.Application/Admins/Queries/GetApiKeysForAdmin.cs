﻿using CSharpVitamins;
using MediatR;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Application.Common.Models;
using RaythaZero.Application.Common.Utils;
using RaythaZero.Domain.ValueObjects;

namespace RaythaZero.Application.Admins.Queries;

public class GetApiKeysForAdmin
{
    public record Query : GetPagedEntitiesInputDto, IRequest<IQueryResponseDto<ListResultDto<ApiKeyDto>>>
    {
        public ShortGuid UserId { get; init; }
        public override string OrderBy { get; init; } = $"CreationTime {SortOrder.ASCENDING}";
    }

    public class Handler : IRequestHandler<Query, IQueryResponseDto<ListResultDto<ApiKeyDto>>>
    {
        private readonly IRaythaDbContext _db;
        public Handler(IRaythaDbContext db)
        {
            _db = db;
        }

        public async Task<IQueryResponseDto<ListResultDto<ApiKeyDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _db.ApiKeys.Where(p => p.UserId == request.UserId.Guid).AsQueryable();

            var total = query.Count();
            var items = query.ApplyPaginationInput(request).Select(ApiKeyDto.GetProjection()).ToArray();

            return new QueryResponseDto<ListResultDto<ApiKeyDto>>(new ListResultDto<ApiKeyDto>(items, total));
        }
    }
}
