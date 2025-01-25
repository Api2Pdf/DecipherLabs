using CSharpVitamins;
using FluentValidation;
using MediatR;
using RaythaZero.Application.Common.Exceptions;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Application.Common.Models;

namespace RaythaZero.Application.Projects.Commands;

public class ManageProject
{
    public record Command : LoggableEntityRequest<CommandResponseDto<ShortGuid>>
    {
        //resumes
        public List<string> Resumes { get; set; }
        
        //travel
        public string TravelDescription { get; set; } = string.Empty;
        public string TravelDescriptionMediaId { get; set; } = string.Empty;
        public int NumberOfTrips { get; set; } = 0;
        public int NumberOfTravelers { get; set; } = 0;
        public string EndUserLocationCity { get; set; } = string.Empty;
        public string EndUserLocationState { get; set; } = string.Empty;
        public bool HasSubcontractorLocation { get; set; } = false;
        public string SubcontractorLocationCity { get; set; } = string.Empty;
        public string SubcontractorLocationState { get; set; } = string.Empty;
        public bool UseRideshare { get; set; } = false;
        public bool UseRentalCar { get; set; } = false;
        
        //materials
        public string MaterialsDescription { get; set; } = string.Empty;
        public string MaterialsDescriptionMediaId { get; set; } = string.Empty;
        
        //equipment
        public string EquipmentDescription { get; set; } = string.Empty;
        public string EquipmentDescriptionMediaId { get; set; } = string.Empty;
        
        //consultants
        public string ConsultantsUrl { get; set; } = string.Empty;
        public string ConsultantsDescription { get; set; } = string.Empty;
        public string ConsultantsDescriptionMediaId { get; set; } = string.Empty;
        
        //subcontractors
        public string SubcontractorsUrl { get; set; } = string.Empty;
        public string SubcontractorsDescription { get; set; } = string.Empty;
        public string SubcontractorsDescriptionMediaId { get; set; } = string.Empty;
        
        //supplies
        public string SuppliesDescription { get; set; } = string.Empty;
        public string SuppliesDescriptionMediaId { get; set; } = string.Empty;
        
        //other direct costs
        public string OtherDirectCostsDescription { get; set; } = string.Empty;
        public string OtherDirectCostsDescriptionMediaId { get; set; } = string.Empty;
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
            var entity = _db.Projects
                .FirstOrDefault(p => p.Id == request.Id.Guid);

            if (entity == null)
                throw new NotFoundException("Project", request.Id);
            
            
            var projectData = entity.ProjectData;
            projectData.Resumes = request.Resumes;
            projectData.Travel.Description = request.TravelDescription;
            projectData.Travel.DescriptionMediaId = request.TravelDescriptionMediaId;
            projectData.Travel.NumberOfTravelers = request.NumberOfTravelers;
            projectData.Travel.NumberOfTrips = request.NumberOfTrips;
            projectData.Travel.EndUserLocationCity = request.EndUserLocationCity;
            projectData.Travel.EndUserLocationState = request.EndUserLocationState;
            projectData.Travel.SubcontractorLocationCity = request.SubcontractorLocationCity;
            projectData.Travel.SubcontractorLocationState = request.SubcontractorLocationState;
            projectData.Travel.HasSubcontractorLocation = request.HasSubcontractorLocation;
            projectData.Travel.UseRideshare = request.UseRideshare;
            projectData.Travel.UseRentalCar = request.UseRentalCar;
            
            projectData.Materials.Description = request.MaterialsDescription;
            projectData.Materials.DescriptionMediaId = request.MaterialsDescriptionMediaId;
            
            projectData.Subcontractor.Url = request.SubcontractorsUrl;
            projectData.Subcontractor.Description = request.SubcontractorsDescription;
            projectData.Subcontractor.DescriptionMediaId = request.SubcontractorsDescriptionMediaId;
            
            projectData.Consultant.Url = request.ConsultantsUrl;
            projectData.Consultant.Description = request.ConsultantsDescription;
            projectData.Consultant.DescriptionMediaId = request.ConsultantsDescriptionMediaId;
            
            projectData.Supplies.Description = request.SuppliesDescription;
            projectData.Supplies.DescriptionMediaId = request.SuppliesDescriptionMediaId;
            
            projectData.Equipment.Description = request.EquipmentDescription;
            projectData.Equipment.DescriptionMediaId = request.EquipmentDescriptionMediaId;
            
            projectData.OtherDirectCosts.Description = request.OtherDirectCostsDescription;
            projectData.OtherDirectCosts.DescriptionMediaId = request.OtherDirectCostsDescriptionMediaId;
            entity.ProjectData = projectData;
            
            _db.Projects.Update(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return new CommandResponseDto<ShortGuid>(entity.Id);
        }
    }
}
