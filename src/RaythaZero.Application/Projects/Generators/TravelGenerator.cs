using Microsoft.SemanticKernel.ChatCompletion;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Domain.Entities;

namespace RaythaZero.Application.Projects.Extractors;

public class TravelGenerator : AbstractGenerator
{
    private readonly IRaythaDbContext _db;
    private readonly IGenerativeAiService _aiService;
    
    public TravelGenerator(
        IRaythaDbContext db,
        IGenerativeAiService aiService)
    {
        _db = db;
        _aiService = aiService;
    }
    
    public override async Task<string> Generate(FinalPackage package)
    {
        // Get and process travel_cost prompt
        var travelCostPrompt = _db.Prompts.First(p => p.DeveloperName == "travel_cost");
        var renderedTravelPrompt = ParsePrompt(travelCostPrompt.PromptText, package);

        ChatHistory travelChatHistory = [];
        travelChatHistory.AddUserMessage(renderedTravelPrompt);
        var travelResponse = await _aiService.GetResponse(travelChatHistory);
        var travelText = travelResponse.First().Content;

        // Get and process flight_cost_generator prompt
        var flightCostPrompt = _db.Prompts.First(p => p.DeveloperName == "flight_cost_generator");
        var renderedFlightPrompt = ParsePrompt(flightCostPrompt.PromptText, package);

        ChatHistory flightChatHistory = [];
        flightChatHistory.AddUserMessage(renderedFlightPrompt);
        var flightResponse = await _aiService.GetResponse(flightChatHistory);
        var flightText = flightResponse.First().Content;

        // Combine both responses
        return $"{travelText}\n\n{flightText}";
    }
}