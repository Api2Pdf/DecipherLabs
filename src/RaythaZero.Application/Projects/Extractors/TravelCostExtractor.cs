using Microsoft.SemanticKernel.ChatCompletion;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Domain.Entities;

namespace RaythaZero.Application.Projects.Extractors;

public class TravelCostExtractor : AbstractExtractor
{
    private readonly IRaythaDbContext _db;
    private readonly IGenerativeAiService _aiService;
    
    public TravelCostExtractor(
        IRaythaDbContext db,
        IGenerativeAiService aiService)
    {
        _db = db;
        _aiService = aiService;
    }
    
    public override async Task<T> Extract<T>(FinalPackage finalPackage)
    {
        // Get travel costs calculations from prompt
        var travelCostPrompt = _db.Prompts.First(p => p.DeveloperName == "calculate_travel_costs");
        var renderedTravelCostPrompt = ParsePrompt(travelCostPrompt.PromptText, finalPackage);

        ChatHistory chatHistory = [];
        chatHistory.AddUserMessage(renderedTravelCostPrompt);
        var travelCostResponse = await _aiService.GetStructuredResponse<T>(chatHistory, GetTravelCostJsonSchema());

        return travelCostResponse;
    }
    
    public override async Task<string> Extract(FinalPackage finalPackage)
    {
        var travelCostPrompt = _db.Prompts.First(p => p.DeveloperName == "calculate_travel_costs");
        var renderedTravelCostPrompt = ParsePrompt(travelCostPrompt.PromptText, finalPackage);

        ChatHistory chatHistory = [];
        chatHistory.AddUserMessage(renderedTravelCostPrompt);
        var travelCostResponse = await _aiService.GetResponse(chatHistory);

        var responseText = travelCostResponse.First().Content;
        return responseText;
    }

    private string GetTravelCostJsonSchema()
    {
        return """
               {
                   "type": "object",
                   "properties": {
                       "LodgingRate": { "type": "number" },
                       "MealRate": { "type": "number" },
                       "FlightCost": { "type": "number" },
                       "RentalCarCost": { "type": ["number", "null"] },
                       "RideshareEstimate": { "type": ["number", "null"] },
                       "SubcontractorFlightCost": { "type": ["number", "null"] },
                       "SubcontractorLodgingRate": { "type": ["number", "null"] },
                       "SubcontractorMealRate": { "type": ["number", "null"] },
                       "NumberOfDays": { "type": "integer" },
                       "NumberOfNights": { "type": "integer" },
                       "TimePeriod": { "type": "string" }
                   },
                   "required": ["LodgingRate", "MealRate", "FlightCost", 
                              "NumberOfDays", "NumberOfNights", "TimePeriod"],
                   "additionalProperties": false
               }
               """;
    }
}
