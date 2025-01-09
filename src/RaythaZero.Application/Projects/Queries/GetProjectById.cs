using MediatR;
using RaythaZero.Application.Common.Exceptions;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Application.Common.Models;

namespace RaythaZero.Application.Projects.Queries;

public class GetProjectById
{
    
    public record Query : GetEntityByIdInputDto, IRequest<IQueryResponseDto<ProjectDto>> 
    {
    }
    
    
    public class Handler : IRequestHandler<Query, IQueryResponseDto<ProjectDto>>
    {
        private readonly IRaythaDbContext _db;
        public Handler(IRaythaDbContext db)
        {
            _db = db;
        }
        
        public async Task<IQueryResponseDto<ProjectDto>> Handle(Query request, CancellationToken cancellationToken)
        {                   
            var entity = _db.Projects
                .FirstOrDefault(p => p.Id == request.Id.Guid);

            if (entity == null)
                throw new NotFoundException("Project", request.Id);
            
            return new QueryResponseDto<ProjectDto>(ProjectDto.GetProjection(entity));
        }
    }
}