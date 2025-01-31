using Microsoft.SemanticKernel.ChatCompletion;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Domain.Entities;
using System.Text.Json;

namespace RaythaZero.Application.Projects.Extractors;

public class ResumeExtractor : AbstractExtractor
{
    private readonly IRaythaDbContext _db;
    private readonly IGenerativeAiService _aiService;
    private readonly string _debugPath = Path.Combine(Directory.GetCurrentDirectory(), "person_debug.txt");
    
    public ResumeExtractor(
        IRaythaDbContext db,
        IGenerativeAiService aiService)
    {
        _db = db;
        _aiService = aiService;
    }
    
    public override async Task<T> Extract<T>(FinalPackage finalPackage)
    {
        File.WriteAllText(_debugPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} Starting resume extraction...\n");
        var result = new List<FinalPackage.IndividualPersonFinalPackage>();
        
        foreach (var individual in finalPackage.individuals)
        {
            File.AppendAllText(_debugPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} Processing resume with text length: {individual.file_text.Length}\n");
            var person = new FinalPackage.IndividualPersonFinalPackage { file_text = individual.file_text };

            // Extract name
            File.AppendAllText(_debugPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} Starting name extraction...\n");
            var namePrompt = _db.Prompts.First(p => p.DeveloperName == "extract_personnel_name");
            File.AppendAllText(_debugPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} Got name prompt: {namePrompt.PromptText}\n");
            var renderedNamePrompt = namePrompt.PromptText;
            ChatHistory nameChat = [];
            nameChat.AddUserMessage($"{renderedNamePrompt}\n\nResume Text:\n{individual.file_text}");
            File.AppendAllText(_debugPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} Sending to AI...\n");

            try {
                var rawResponse = (await _aiService.GetResponse(nameChat)).First().Content;
                var nameResponse = JsonSerializer.Serialize(rawResponse);
                File.AppendAllText(_debugPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} Got response: {nameResponse}\n");
                person.name = nameResponse;
            } catch (Exception ex) {
                File.AppendAllText(_debugPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} Error: {ex.Message}\n{ex.StackTrace}\n");
                throw;
            }

            // Extract job title
            var jobTitlePrompt = _db.Prompts.First(p => p.DeveloperName == "extract_job_title");
            var renderedJobTitlePrompt = jobTitlePrompt.PromptText;
            ChatHistory jobTitleChat = [];
            jobTitleChat.AddUserMessage($"{renderedJobTitlePrompt}\n\nResume Text:\n{individual.file_text}");
            var rawJobTitleResponse = (await _aiService.GetResponse(jobTitleChat)).First().Content;
            var jobTitleResponse = JsonSerializer.Serialize(rawJobTitleResponse);
            person.job_title = jobTitleResponse;
            File.AppendAllText(_debugPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} Extracted Job Title: {jobTitleResponse}\n");

            // Extract graduation year
            var gradYearPrompt = _db.Prompts.First(p => p.DeveloperName == "extract_graduation_year");
            var renderedGradYearPrompt = gradYearPrompt.PromptText;
            ChatHistory gradYearChat = [];
            gradYearChat.AddUserMessage($"{renderedGradYearPrompt}\n\nResume Text:\n{individual.file_text}");
            var rawGradYearResponse = (await _aiService.GetResponse(gradYearChat)).First().Content;
            var gradYearResponse = JsonSerializer.Serialize(rawGradYearResponse);
            person.grad_year = gradYearResponse;
            File.AppendAllText(_debugPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} Extracted Graduation Year: {gradYearResponse}\n");

            // Extract BLS code
            var blsCodePrompt = _db.Prompts.First(p => p.DeveloperName == "extract_bls_code");
            var renderedBlsCodePrompt = blsCodePrompt.PromptText;
            ChatHistory blsCodeChat = [];
            blsCodeChat.AddUserMessage($"{renderedBlsCodePrompt}\n\nResume Text:\n{individual.file_text}");
            var rawBlsCodeResponse = (await _aiService.GetResponse(blsCodeChat)).First().Content;
            var blsCodeResponse = JsonSerializer.Serialize(rawBlsCodeResponse);
            person.bls_code = blsCodeResponse;
            File.AppendAllText(_debugPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} Extracted BLS Code: {blsCodeResponse}\n");

            result.Add(person);
        }

        File.AppendAllText(_debugPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} Finished processing {result.Count} resumes\n");
        return (T)(object)result;
    }

    public override async Task<string> Extract(FinalPackage finalPackage)
    {
        throw new NotImplementedException();
    }

    private string GetResumeJsonSchema()
    {
        return """
               {
                   "type": "array",
                   "items": {
                       "type": "object",
                       "properties": {
                           "name": { "type": "string" },
                           "grad_year": { "type": "string" },
                           "job_title": { "type": "string" },
                           "bls_code": { "type": "string" }
                       },
                       "additionalProperties": false
                   }
               }
               """;
    }
}
