using Microsoft.SemanticKernel.ChatCompletion;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Domain.Entities;
using System.Text.Json;

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
        throw new NotImplementedException();
    }

    public override async Task<string> Extract(FinalPackage finalPackage)
    {
        var travelCostPrompt = _db.Prompts.First(p => p.DeveloperName == "calculate_travel_costs");
        var renderedTravelCostPrompt = ParsePrompt(travelCostPrompt.PromptText, finalPackage);

        ChatHistory chatHistory = [];
        chatHistory.AddUserMessage(renderedTravelCostPrompt);
        var travelCostResponse = await _aiService.GetStructuredResponse<FinalPackage.TravelCostInfo>(chatHistory, GetTravelCostJsonSchema());
        
        try
        {
            var json = JsonSerializer.Serialize(travelCostResponse);
            JsonDocument.Parse(json);
            return json;
        }
        catch (JsonException)
        {
            throw new Exception("Invalid JSON returned from travel cost calculation");
        }
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
