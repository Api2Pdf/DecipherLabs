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
using Microsoft.Extensions.Configuration;
using RaythaZero.Domain.ValueObjects;

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
      private readonly BenefitsExtractor _benefitsExtractor;
      private readonly FringeGenerator _fringeGenerator;
      private readonly TravelGenerator _travelGenerator;
      private readonly IConfiguration _configuration;
      private readonly string _debugPath = "person_debug.txt";
      
      public BackgroundTask(
          IRaythaDbContext db,
          IFileStorageProvider fileStorageProvider,
          IFileStorageProviderSettings fileStorageProviderSettings,
          IGenerativeAiService aiService,
          HttpClient httpClient,
          ResumeExtractor resumeExtractor,
          PersonGenerator personGenerator,
          BenefitsExtractor benefitsExtractor,
          FringeGenerator fringeGenerator,
          TravelGenerator travelGenerator,
          IConfiguration configuration
      )
      {
          _db = db;
          _fileStorageProviderSettings = fileStorageProviderSettings;
          _fileStorageProvider = fileStorageProvider;
          _aiService = aiService;
          _httpClient = httpClient;
          _resumeExtractor = resumeExtractor;
          _personGenerator = personGenerator;
          _benefitsExtractor = benefitsExtractor;
          _fringeGenerator = fringeGenerator;
          _travelGenerator = travelGenerator;
          _configuration = configuration;
      }

      public async Task Execute(Guid jobId, JsonElement args, CancellationToken cancellationToken)
      {
          var job = _db.BackgroundTasks.First(p => p.Id == jobId);
          
          //Get company level data
          var company = _db.OrganizationSettings.First();
          
          //Get current project and its associated data
          var projectId = new ShortGuid(args.GetProperty("Id").GetString());
          var project = _db.Projects.First(p => p.Id == projectId.Guid);
          
          //Get Topic Information from JSON file
          var topic = ProjectUtils.GetTopics()
              .FirstOrDefault(p => p.topic_number == project.ProjectData.TopicNumber);
          
          if (topic == null)
              throw new Exception($"Topic {project.ProjectData.TopicNumber} does not exist");
          
          var finalPackage = await InitializeFinalPackage(company.CompanySetupData, project.ProjectData, topic);

          // Cost Supplement Title Page
          var titlePagePrompt = _db.Prompts.First(p => p.DeveloperName == "cost_supplement_title_page");
          var renderedTitlePagePrompt = ParsePrompt(titlePagePrompt.PromptText, finalPackage);
          ChatHistory titlePageChat = [];
          titlePageChat.AddUserMessage(renderedTitlePagePrompt);
          var titlePageResponse = await _aiService.GetResponse(titlePageChat);
          var titlePageText = titlePageResponse.First().Content;
          await UpdateStatus(job, titlePageText + "\n\n", 5, cancellationToken);

          // Introduction Paragraph
          finalPackage.total_cost = 500000M;  // Hardcoded placeholder
          finalPackage.truncated_project_description = "missile technology";  // Hardcoded placeholder

          var introductionPrompt = _db.Prompts.First(p => p.DeveloperName == "introduction");
          var renderedIntroductionPrompt = ParsePrompt(introductionPrompt.PromptText, finalPackage);
          ChatHistory introductionChat = [];
          introductionChat.AddUserMessage(renderedIntroductionPrompt);
          var introductionResponse = await _aiService.GetResponse(introductionChat);
          var introductionText = introductionResponse.First().Content;
          await UpdateStatus(job, introductionText + "\n\n", 7, cancellationToken);

          // Direct Labor Header
          var laborHeaderPrompt = _db.Prompts.First(p => p.DeveloperName == "direct_labor_header");
          var renderedLaborHeaderPrompt = ParsePrompt(laborHeaderPrompt.PromptText, finalPackage);
          ChatHistory laborHeaderChat = [];
          laborHeaderChat.AddUserMessage(renderedLaborHeaderPrompt);
          var laborHeaderResponse = await _aiService.GetResponse(laborHeaderChat);
          var laborHeaderText = laborHeaderResponse.First().Content;
          await UpdateStatus(job, laborHeaderText + "\n\n", 9, cancellationToken);

          // Resume Processing
          File.AppendAllText(_debugPath, "\nBeginToGeneratePackage: Starting Resume Processing...\n");
          finalPackage.individuals = await _resumeExtractor.Extract<List<FinalPackage.IndividualPersonFinalPackage>>(finalPackage);

          // Set hardcoded wage rate for each person
          foreach (var person in finalPackage.individuals)
          {
              person.wage_rate = "150.00";
          }

          File.AppendAllText(_debugPath, $"BeginToGeneratePackage: Processed {finalPackage.individuals.Count} resumes\n");

          foreach (var person in finalPackage.individuals)
          {
              File.AppendAllText(_debugPath, $"BeginToGeneratePackage: Person - Name: {person.name}, Job: {person.job_title}, BLS: {person.bls_code}, Grad Year: {person.grad_year}\n");
          }

          // Generate key personnel section
          File.AppendAllText(_debugPath, "\nBeginToGeneratePackage: Starting Key Personnel Generation...\n");
          var completePersonnelText = await _personGenerator.Generate(finalPackage);
          await UpdateStatus(job, completePersonnelText + "\n\n", 15, cancellationToken);

          // Direct Labor Amount
          finalPackage.direct_labor_amount = 250000M;  // Hardcoded placeholder

          var laborAmountPrompt = _db.Prompts.First(p => p.DeveloperName == "direct_labor_amount");
          var renderedLaborAmountPrompt = ParsePrompt(laborAmountPrompt.PromptText, finalPackage);
          ChatHistory laborAmountChat = [];
          laborAmountChat.AddUserMessage(renderedLaborAmountPrompt);
          var laborAmountResponse = await _aiService.GetResponse(laborAmountChat);
          var laborAmountText = laborAmountResponse.First().Content;
          await UpdateStatus(job, laborAmountText + "\n\n", 17, cancellationToken);

          // Benefits and Fringe Processing
          finalPackage.offers_benefits_extracted = await _benefitsExtractor.Extract(finalPackage);
          finalPackage.fringe_rate = 0.15M;  
          finalPackage.fully_loaded_labor_amount = 100M;
          finalPackage.fringe_rate_generated = await _fringeGenerator.Generate(finalPackage);
          await UpdateStatus(job, finalPackage.fringe_rate_generated + "\n\n", 10, cancellationToken);

          // Travel Cost Processing
          File.AppendAllText(_debugPath, "\nBeginToGeneratePackage: Starting Travel Cost Processing...\n");
          File.AppendAllText(_debugPath, $"Configuration has AmadeusSettings:ApiKey: {!string.IsNullOrEmpty(_configuration["AmadeusSettings:ApiKey"])}\n");
          File.AppendAllText(_debugPath, $"Configuration has AmadeusSettings:ApiSecret: {!string.IsNullOrEmpty(_configuration["AmadeusSettings:ApiSecret"])}\n");

          var travelCostExtractor = new TravelCostExtractor(_configuration);
          var travelCostInfo = await travelCostExtractor.Extract(finalPackage);
          await UpdateStatus(job, travelCostInfo + "\n\n", 20, cancellationToken);
          finalPackage.travel_cost = JsonSerializer.Deserialize<FinalPackage.TravelCostInfo>(travelCostInfo);
          finalPackage.travel_cost_writeup = await _travelGenerator.Generate(finalPackage);
          await UpdateStatus(job, finalPackage.travel_cost_writeup + "\n\n", 30, cancellationToken);

          // Update just the travel cost properties while preserving other data
          project.ProjectData.Travel.TravelCost = finalPackage.travel_cost;
          project.ProjectData.Travel.TravelCostWriteup = finalPackage.travel_cost_writeup;
          _db.Projects.Update(project);

          await UpdateStatus(job, "Done", 100, cancellationToken);
          await _db.SaveChangesAsync(cancellationToken);
      }

      private async Task UpdateStatus(Domain.Entities.BackgroundTask job, string status, int percentComplete, CancellationToken cancellationToken, bool appendStatus = true)
      {
          job.StatusInfo = appendStatus ? job.StatusInfo + "<br/>" + status : status;
          job.PercentComplete = percentComplete;
          _db.BackgroundTasks.Update(job);
          await _db.SaveChangesAsync(cancellationToken);
      }

      private async Task<FinalPackage> InitializeFinalPackage(CompanyLevelInfo companyData, ProjectLevelInfo projectData, Topic topic)
      {
          var finalPackage = new FinalPackage
          {
              company_name = companyData.LegalName,
              company_url = companyData.Url,
              company_city_hq = companyData.CityHq,
              company_state_hq = companyData.StateHq,
              offers_benefits = companyData.OffersBenefits,
              offers_benefits_description = companyData.OffersBenefitsDescription,
              dsip_proposal_number = projectData.DsipProposalNumber,
              topic_number = projectData.TopicNumber,
              topic = topic,
              type_of_proposal = projectData.TypeOfProposal,
              other_direct_cost_selections = projectData.OtherDirectCostSelections,
              travel = new()
              {
                  use_rental = projectData.Travel.UseRentalCar,
                  use_rideshare = projectData.Travel.UseRideshare,
                  number_of_travelers = projectData.Travel.NumberOfTravelers,
                  number_of_trips = projectData.Travel.NumberOfTrips,
                  end_user_location_city = projectData.Travel.EndUserLocationCity,
                  end_user_location_state = projectData.Travel.EndUserLocationState,
                  has_subcontractor_location = projectData.Travel.HasSubcontractorLocation,
                  subcontrator_location_city = projectData.Travel.SubcontractorLocationCity,
                  subcontractor_location_state = projectData.Travel.SubcontractorLocationState
              },
              materials = new()
              {
                  description = projectData.Materials.Description
              },
              equipment = new()
              {
                  description = projectData.Equipment.Description
              },
              supplies = new()
              {
                  description = projectData.Supplies.Description
              },
              consultant = new()
              {
                  description = projectData.Consultant.Description,
                  url = projectData.Consultant.Url,
              },
              subcontractor = new()
              {
                  description = projectData.Subcontractor.Description,
                  url = projectData.Subcontractor.Url,
              },
              other_direct_costs = new()
              {
                  description = projectData.OtherDirectCosts.Description
              },
              bls_data = ProjectUtils.GetBlsData(companyData.StateHq)
          };
          
          if (!string.IsNullOrEmpty(companyData.WageRateSheetMediaId))
          {
              var fileText = await GetFileContent(companyData.WageRateSheetMediaId);
              finalPackage.wage_rate_sheet_file_text = fileText;
          }
          
          if (!string.IsNullOrEmpty(companyData.PreviousCostVolumeExcelMediaId))
          {
              var fileText = await GetFileContent(companyData.PreviousCostVolumeExcelMediaId);
              finalPackage.previous_cost_volumes_excel_file_text = fileText;
          }
          
          if (!string.IsNullOrEmpty(companyData.PreviousCostVolumeWordMediaId))
          {
              var fileText = await GetFileContent(companyData.PreviousCostVolumeWordMediaId);
              finalPackage.previous_cost_volumes_word_file_text = fileText;
          }
          
          if (!string.IsNullOrEmpty(companyData.BalanceSheetMediaId))
          {
              var fileText = await GetFileContent(companyData.BalanceSheetMediaId);
              finalPackage.balance_sheet_file_text = fileText;
          }
          
          if (!string.IsNullOrEmpty(companyData.ProfitAndLossMediaId))
          {
              var fileText = await GetFileContent(companyData.ProfitAndLossMediaId);
              finalPackage.profit_and_loss_file_text = fileText;
          }

          foreach (var resumeMediaId in projectData.Resumes)
          {
              var resumeText = await GetFileContent(resumeMediaId);
              if (string.IsNullOrEmpty(resumeText))
                  continue;
              
              finalPackage.individuals.Add(new FinalPackage.IndividualPersonFinalPackage()
              {
                  file_text = resumeText
              });
          }

          if (!string.IsNullOrEmpty(projectData.Travel.DescriptionMediaId))
          {
              var fileText = await GetFileContent(projectData.Travel.DescriptionMediaId);
              finalPackage.travel.file_text = fileText;
          }
          
          if (!string.IsNullOrEmpty(projectData.Materials.DescriptionMediaId))
          {
              var fileText = await GetFileContent(projectData.Materials.DescriptionMediaId);
              finalPackage.materials.file_text = fileText;
          }
          
          if (!string.IsNullOrEmpty(projectData.Equipment.DescriptionMediaId))
          {
              var fileText = await GetFileContent(projectData.Equipment.DescriptionMediaId);
              finalPackage.equipment.file_text = fileText;
          }
          
          if (!string.IsNullOrEmpty(projectData.Supplies.DescriptionMediaId))
          {
              var fileText = await GetFileContent(projectData.Supplies.DescriptionMediaId);
              finalPackage.supplies.file_text = fileText;
          }
          
          if (!string.IsNullOrEmpty(projectData.Consultant.DescriptionMediaId))
          {
              var fileText = await GetFileContent(projectData.Consultant.DescriptionMediaId);
              finalPackage.consultant.file_text = fileText;
          }
          
          if (!string.IsNullOrEmpty(projectData.Subcontractor.DescriptionMediaId))
          {
              var fileText = await GetFileContent(projectData.Subcontractor.DescriptionMediaId);
              finalPackage.subcontractor.file_text = fileText;
          }
          
          if (!string.IsNullOrEmpty(projectData.OtherDirectCosts.DescriptionMediaId))
          {
              var fileText = await GetFileContent(projectData.OtherDirectCosts.DescriptionMediaId);
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

      private string ParsePrompt(string promptText, object data)
      {
          var parser = new FluidParser();
          var template = parser.Parse(promptText);
          var context = new TemplateContext(data);
          return template.Render(context);
      }
  }
}
