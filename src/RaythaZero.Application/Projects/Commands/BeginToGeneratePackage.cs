using System.Net.Mime;
using System.Text.Json;
using CSharpVitamins;
using Fluid;
using MediatR;
using Microsoft.SemanticKernel.ChatCompletion;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Application.Common.Models;
using RaythaZero.Application.Projects.Extractors;
using RaythaZero.Application.Projects.Utils;
using RaythaZero.Domain.Entities;

namespace RaythaZero.Application.Projects.Commands;

public class BeginToGeneratePackage
{

    public record Command : LoggableEntityRequest<CommandResponseDto<ShortGuid>>
    {
    }
    
    public class Handler : IRequestHandler<Command, CommandResponseDto<ShortGuid>>
    {
        
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IRaythaDbContext _db;
        public Handler(
            IBackgroundTaskQueue taskQueue,
            IRaythaDbContext db
        )
        {
            _taskQueue = taskQueue;
            _db = db;
        }
        public async Task<CommandResponseDto<ShortGuid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var backgroundJobId = await _taskQueue.EnqueueAsync<BackgroundTask>(request, cancellationToken);
            return new CommandResponseDto<ShortGuid>(backgroundJobId);
        }
    }

    public class BackgroundTask : IExecuteBackgroundTask
    {
        private readonly IRaythaDbContext _db;
        private readonly IFileStorageProviderSettings _fileStorageProviderSettings;
        private readonly IFileStorageProvider _fileStorageProvider;
        private readonly IGenerativeAiService _aiService;
        private readonly HttpClient _httpClient;
        private readonly ResumeExtractor _resumeExtractor;
        private readonly PersonGenerator _personGenerator;
        
        public BackgroundTask(
            IRaythaDbContext db,
            IFileStorageProvider fileStorageProvider,
            IFileStorageProviderSettings fileStorageProviderSettings,
            IGenerativeAiService aiService,
            HttpClient httpClient,
            ResumeExtractor resumeExtractor,
            PersonGenerator personGenerator
        )
        {
            _db = db;
            _fileStorageProviderSettings = fileStorageProviderSettings;
            _fileStorageProvider = fileStorageProvider;
            _aiService = aiService;
            _httpClient = httpClient;
            _resumeExtractor = resumeExtractor;
            _personGenerator = personGenerator;
        }

        public async Task Execute(Guid jobId, JsonElement args, CancellationToken cancellationToken)
        {
            var job = _db.BackgroundTasks.First(p => p.Id == jobId);
            // await UpdateStatus(job, "Getting started...", 5, cancellationToken);
            
            //Get company level data
            var company = _db.OrganizationSettings.First();
            // await UpdateStatus(job, company._CompanySetupData ?? string.Empty, 5, cancellationToken);
            
            //Get current project and its associated data
            var projectId = new ShortGuid(args.GetProperty("Id").GetString());
            var project = _db.Projects.First(p => p.Id == projectId.Guid);
            // await UpdateStatus(job, project._ProjectData ?? string.Empty, 5, cancellationToken);
            
            //Get Topic Information from JSON file
            var topic = ProjectUtils.GetTopics()
                .FirstOrDefault(p => p.topic_number == project.ProjectData.TopicNumber);
            
            if (topic == null)
                throw new Exception($"Topic {project.ProjectData.TopicNumber} does not exist");
            
            // await UpdateStatus(job, JsonSerializer.Serialize(topic), 5, cancellationToken);
            
            // await UpdateStatus(job, "Pulling project data...", 5, cancellationToken);
            var finalPackage = await InitializeFinalPackage(company.CompanySetupData, project.ProjectData, topic);
            // await UpdateStatus(job, JsonSerializer.Serialize(finalPackage), 5, cancellationToken);
            
            //Extract Resumes Data
            // await UpdateStatus(job, "Processing resumes...", 10, cancellationToken);
            finalPackage.individuals = await _resumeExtractor.Extract<List<FinalPackage.IndividualPersonFinalPackage>>(finalPackage);
            // await UpdateStatus(job, JsonSerializer.Serialize(JsonSerializer.Serialize(finalPackage.individuals)), 50, cancellationToken);
            
            //Generate content from resumes
            var personGenerationContent = await _personGenerator.Generate(finalPackage);
            // await UpdateStatus(job, personGenerationContent, 100, cancellationToken);
            
            await _db.SaveChangesAsync(cancellationToken);
        } 

        private async Task UpdateStatus(Domain.Entities.BackgroundTask job, string status, int percentComplete, CancellationToken cancellationToken, bool appendStatus = true)
        {
            job.StatusInfo = appendStatus ? job.StatusInfo + "<br/>" + status : status;
            job.PercentComplete = percentComplete;
            _db.BackgroundTasks.Update(job);
            await _db.SaveChangesAsync(cancellationToken);
        }

        private async Task<FinalPackage> InitializeFinalPackage(CompanyLevelInfo company, ProjectLevelInfo project, Topic topic)
        {
            FinalPackage finalPackage = new FinalPackage
            {
                company_name = company.LegalName,
                company_url = company.Url,
                company_city_hq = company.CityHq,
                company_state_hq = company.StateHq,
                offers_benefits = company.OffersBenefits,
                offers_benefits_description = company.OffersBenefitsDescription,
                dsip_proposal_number = project.DsipProposalNumber,
                topic_number = project.TopicNumber,
                topic = topic,
                type_of_proposal = project.TypeOfProposal,
                other_direct_cost_selections = project.OtherDirectCostSelections,
                travel = new()
                {
                    use_rental = project.Travel.UseRentalCar,
                    use_rideshare = project.Travel.UseRideshare,
                    number_of_travelers = project.Travel.NumberOfTravelers,
                    number_of_trips = project.Travel.NumberOfTrips,
                    location_of_gov_end_user = project.Travel.LocationOfGovEndUser,
                    location_of_subcontractor = project.Travel.LocationOfSubcontractor
                },
                materials = new()
                {
                    description = project.Materials.Description
                },
                equipment = new()
                {
                    description = project.Equipment.Description
                },
                supplies = new()
                {
                    description = project.Supplies.Description
                },
                consultant = new()
                {
                    description = project.Consultant.Description,
                    url = project.Consultant.Url,
                },
                subcontractor = new()
                {
                    description = project.Subcontractor.Description,
                    url = project.Subcontractor.Url,
                },
                other_direct_costs = new()
                {
                    description = project.OtherDirectCosts.Description
                }
            };
            
            if (!string.IsNullOrEmpty(company.WageRateSheetMediaId))
            {
                var fileText = await GetFileContent(company.WageRateSheetMediaId);
                finalPackage.wage_rate_sheet_file_text = fileText;
            }
            
            if (!string.IsNullOrEmpty(company.PreviousCostVolumeExcelMediaId))
            {
                var fileText = await GetFileContent(company.PreviousCostVolumeExcelMediaId);
                finalPackage.previous_cost_volumes_excel_file_text = fileText;
            }
            
            if (!string.IsNullOrEmpty(company.PreviousCostVolumeWordMediaId))
            {
                var fileText = await GetFileContent(company.PreviousCostVolumeWordMediaId);
                finalPackage.previous_cost_volumes_word_file_text = fileText;
            }
            
            if (!string.IsNullOrEmpty(company.BalanceSheetMediaId))
            {
                var fileText = await GetFileContent(company.BalanceSheetMediaId);
                finalPackage.balance_sheet_file_text = fileText;
            }
            
            if (!string.IsNullOrEmpty(company.ProfitAndLossMediaId))
            {
                var fileText = await GetFileContent(company.ProfitAndLossMediaId);
                finalPackage.profit_and_loss_file_text = fileText;
            }

            foreach (var resumeMediaId in project.Resumes)
            {
                var resumeText = await GetFileContent(resumeMediaId);
                if (string.IsNullOrEmpty(resumeText))
                    continue;
                
                finalPackage.individuals.Add(new FinalPackage.IndividualPersonFinalPackage()
                {
                    file_text = resumeText
                });
            }

            if (!string.IsNullOrEmpty(project.Travel.DescriptionMediaId))
            {
                var fileText = await GetFileContent(project.Travel.DescriptionMediaId);
                finalPackage.travel.file_text = fileText;
            }
            
            if (!string.IsNullOrEmpty(project.Materials.DescriptionMediaId))
            {
                var fileText = await GetFileContent(project.Materials.DescriptionMediaId);
                finalPackage.materials.file_text = fileText;
            }
            
            if (!string.IsNullOrEmpty(project.Equipment.DescriptionMediaId))
            {
                var fileText = await GetFileContent(project.Equipment.DescriptionMediaId);
                finalPackage.equipment.file_text = fileText;
            }
            
            if (!string.IsNullOrEmpty(project.Supplies.DescriptionMediaId))
            {
                var fileText = await GetFileContent(project.Supplies.DescriptionMediaId);
                finalPackage.supplies.file_text = fileText;
            }
            
            if (!string.IsNullOrEmpty(project.Consultant.DescriptionMediaId))
            {
                var fileText = await GetFileContent(project.Consultant.DescriptionMediaId);
                finalPackage.consultant.file_text = fileText;
            }
            
            if (!string.IsNullOrEmpty(project.Subcontractor.DescriptionMediaId))
            {
                var fileText = await GetFileContent(project.Subcontractor.DescriptionMediaId);
                finalPackage.subcontractor.file_text = fileText;
            }
            
            if (!string.IsNullOrEmpty(project.OtherDirectCosts.DescriptionMediaId))
            {
                var fileText = await GetFileContent(project.OtherDirectCosts.DescriptionMediaId);
                finalPackage.other_direct_costs.file_text = fileText;
            }

            return finalPackage;
        }

        private async Task<string> GetFileContent(string mediaId)
        {
            string fileText = string.Empty;
            var mediaItem = _db.MediaItems.FirstOrDefault(p => p.Id == new ShortGuid(mediaId).Guid);
            if (mediaItem != null)
            {
                var downloadUrl = await _fileStorageProvider.GetDownloadUrlAsync(mediaItem.ObjectKey, DateTime.UtcNow.AddDays(1));
                (var resumeBytes, var mimeType) = await DownloadFileAsync(downloadUrl);
                fileText = ProjectUtils.GetTextFromDocument(resumeBytes, mediaItem.FileName);
            }
            return fileText;
        }
        
        private async Task<(byte[] FileBytes, string MimeType)> DownloadFileAsync(string fileUrl)
        {
            // Download the file
            var response = await _httpClient.GetAsync(fileUrl);
            response.EnsureSuccessStatusCode();

            // Read file content as byte array
            var fileBytes = await response.Content.ReadAsByteArrayAsync();

            // Extract file extension from the URL
            var fileExtension = Path.GetExtension(fileUrl);

            // Detect MIME type
            var mimeType = GetMimeType(fileExtension);

            return (fileBytes, mimeType);
        }

        private string GetMimeType(string fileExtension)
        {
            if (string.IsNullOrEmpty(fileExtension))
                return MediaTypeNames.Application.Octet;

            switch (fileExtension)
            {
                case ".pdf":
                    return MediaTypeNames.Application.Pdf;
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".txt":
                    return "text/plain";
                case ".html":
                    return "text/html";
                default:
                    return MediaTypeNames.Application.Octet;
            }
        }

    }
}