﻿using RaythaZero.Application.Common.Models;
using RaythaZero.Domain.Entities;
using System.Linq.Expressions;

namespace RaythaZero.Application.Admins;

public record ApiKeyDto : BaseEntityDto
{
    public Guid? CreatorUserId { get; init; }
    public AuditableUserDto? CreatorUser { get; init; }
    public DateTime? CreationTime { get; init; }

    public static Expression<Func<ApiKey, ApiKeyDto>> GetProjection()
    {
        return entity => GetProjection(entity);
    }

    public static ApiKeyDto GetProjection(ApiKey entity)
    {
        return new ApiKeyDto
        {
            Id = entity.Id,
            CreatorUserId = entity.CreatorUserId,
            CreationTime = entity.CreationTime,
            CreatorUser = AuditableUserDto.GetProjection(entity.CreatorUser)
        };
    }
}
