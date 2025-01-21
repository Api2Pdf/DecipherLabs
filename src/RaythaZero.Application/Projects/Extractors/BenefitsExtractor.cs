using Microsoft.SemanticKernel.ChatCompletion;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Domain.Entities;

namespace RaythaZero.Application.Projects.Extractors;

public class BenefitsExtractor : AbstractExtractor
{
    private readonly IRaythaDbContext _db;
    private readonly IGenerativeAiService _aiService;
    
    public BenefitsExtractor(
        IRaythaDbContext db,
        IGenerativeAiService aiService)
    {
        _db = db;
        _aiService = aiService;
    }
    
    public override async Task<T> Extract<T>(FinalPackage finalPackage)
    {
        var benefitsExtractionPrompt = _db.Prompts.First(p => p.DeveloperName == "parse_benefits");
        var renderedBenefitsPrompt = ParsePrompt(benefitsExtractionPrompt.PromptText, finalPackage);

        ChatHistory chatHistory = [];
        chatHistory.AddUserMessage(renderedBenefitsPrompt);
        var benefitsPromptResponse = await _aiService.GetStructuredResponse<T>(chatHistory, GetBenefitsJsonSchema());
        return benefitsPromptResponse;
    }

    private string GetBenefitsJsonSchema()
    {
        return """
               {
                   "type": "object",
                   "properties": {
                       "benefits_list": { "type": "string" }
                   },
                   "required": ["benefits_list"],
                   "additionalProperties": false
               }
               """;
    }
}