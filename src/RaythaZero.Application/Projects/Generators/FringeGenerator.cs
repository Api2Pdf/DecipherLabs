using Microsoft.SemanticKernel.ChatCompletion;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Domain.Entities;

namespace RaythaZero.Application.Projects.Extractors;

public class FringeGenerator : AbstractGenerator
{
    private readonly IRaythaDbContext _db;
    private readonly IGenerativeAiService _aiService;
    
    public FringeGenerator(
        IRaythaDbContext db,
        IGenerativeAiService aiService)
    {
        _db = db;
        _aiService = aiService;
    }
    
    public override async Task<string> Generate(FinalPackage package)
    {
        var prompt = _db.Prompts.First(p => p.DeveloperName == "fringe_rate");
        var renderedPrompt = ParsePrompt(prompt.PromptText, package);

        ChatHistory chatHistory = [];
        chatHistory.AddUserMessage(renderedPrompt);
        
        var response = await _aiService.GetResponse(chatHistory);
        var responseText = response.First().Content;
        return responseText;
    }
}