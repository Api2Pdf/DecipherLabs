using Microsoft.SemanticKernel.ChatCompletion;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Domain.Entities;

namespace RaythaZero.Application.Projects.Extractors;

public class PersonGenerator : AbstractGenerator
{
    private readonly IRaythaDbContext _db;
    private readonly IGenerativeAiService _aiService;
    
    public PersonGenerator(
        IRaythaDbContext db,
        IGenerativeAiService aiService)
    {
        _db = db;
        _aiService = aiService;
    }
    
    public override async Task<string> Generate(FinalPackage finalPackage) 
    {
        var resumeGenerationPrompts = _db.Prompts.First(p => p.DeveloperName == "person_generation");
        var renderedResumeGenerationPrompt = ParsePrompt(resumeGenerationPrompts.PromptText, finalPackage);

        ChatHistory chatHistory = [];
        chatHistory.AddUserMessage(renderedResumeGenerationPrompt);
        var resumeGenerationResponse = await _aiService.GetResponse(chatHistory);
        var resumeGenerationText = resumeGenerationResponse.First().Content;
        return resumeGenerationText;
    }
}