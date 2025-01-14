using FluentValidation;
using MediatR;
using RaythaZero.Application.Common.Exceptions;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Application.Common.Models;
using RaythaZero.Application.Common.Utils;

namespace RaythaZero.Application.AuthenticationSchemes.Queries;

public class GetAuthenticationSchemeByName
{
    public record Query : IRequest<IQueryResponseDto<AuthenticationSchemeDto>> 
    {
        public string DeveloperName { get; init; } = null!;
    }

    public class Handler : IRequestHandler<Query, IQueryResponseDto<AuthenticationSchemeDto>>
    {
        private readonly IRaythaDbContext _db;
        public Handler(IRaythaDbContext db)
        {
            _db = db;
        }
        
        public async Task<IQueryResponseDto<AuthenticationSchemeDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var entity = _db.AuthenticationSchemes.FirstOrDefault(p => p.DeveloperName == request.DeveloperName.ToDeveloperName());

            if (entity == null)
                throw new NotFoundException("Authentication Scheme", request.DeveloperName.ToDeveloperName());

            return new QueryResponseDto<AuthenticationSchemeDto>(AuthenticationSchemeDto.GetProjection(entity));
        }
    }
}