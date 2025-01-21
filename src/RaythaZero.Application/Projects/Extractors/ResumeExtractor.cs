using Microsoft.SemanticKernel.ChatCompletion;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Domain.Entities;

namespace RaythaZero.Application.Projects.Extractors;

public class ResumeExtractor : AbstractExtractor
{
    private readonly IRaythaDbContext _db;
    private readonly IGenerativeAiService _aiService;
    
    public ResumeExtractor(
        IRaythaDbContext db,
        IGenerativeAiService aiService)
    {
        _db = db;
        _aiService = aiService;
    }
    
    public override async Task<T> Extract<T>(FinalPackage finalPackage)
    {
        var resumeExtractionPrompts = _db.Prompts.First(p => p.DeveloperName == "bls_code");
        var renderedResumeExtractionPrompt = ParsePrompt(resumeExtractionPrompts.PromptText, finalPackage);

        ChatHistory chatHistory = [];
        chatHistory.AddUserMessage(renderedResumeExtractionPrompt);
        var resumePromptResponse = await _aiService.GetStructuredResponse<T>(chatHistory, GetResumeJsonSchema());
        return resumePromptResponse;
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
                           "job_title": { "type": "string" },,
                           "bls_code": { "type": "string" },
                       },
                       "additionalProperties": false
                   }
               }
               """;
    }
}
