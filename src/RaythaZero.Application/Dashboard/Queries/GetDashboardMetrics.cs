﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Application.Common.Models;

namespace RaythaZero.Application.Dashboard.Queries;

public class GetDashboardMetrics
{
    public record Query : IRequest<IQueryResponseDto<DashboardDto>>
    {
    }

    public class Handler : IRequestHandler<Query, IQueryResponseDto<DashboardDto>>
    {
        private readonly IRaythaDbContext _db;
        public readonly IRaythaRawDbInfo _rawSqlDb;
        public Handler(IRaythaDbContext db, IRaythaRawDbInfo rawSqlDb)
        {
            _rawSqlDb = rawSqlDb;
            _db = db;
        }

        public async Task<IQueryResponseDto<DashboardDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            int totalUsers = await _db.Users.CountAsync();
            long totalFileStorageSize = await _db.MediaItems.SumAsync(p => p.Length);
            var dbSize = _rawSqlDb.GetDatabaseSize();

            decimal numericValueOfReserved = Convert.ToDecimal(dbSize.reserved.Split(" ").First());
            string units = dbSize.reserved.Split(" ").Last();
            decimal dbSizeInMb = ComputeToMb(numericValueOfReserved, units);
            return new QueryResponseDto<DashboardDto>(
                new DashboardDto
                {
                    TotalUsers = totalUsers,
                    FileStorageSize = totalFileStorageSize,
                    DbSize = dbSizeInMb
                });
        }

        private decimal ComputeToMb(decimal rawValue, string units)
        {
            switch(units)
            {
                case "KB":
                    return rawValue / 1000;
                case "GB":
                    return rawValue * 1000;
                default:
                    return rawValue;
            }
        }
    }
}
