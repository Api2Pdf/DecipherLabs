using Microsoft.SemanticKernel.ChatCompletion;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Domain.Entities;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace RaythaZero.Application.Projects.Extractors;

public class TravelCostExtractor : AbstractExtractor
{
    private readonly IRaythaDbContext _db;
    private readonly IGenerativeAiService _aiService;
    private readonly IConfiguration _configuration;

    private const string TokenUrl = "https://test.api.amadeus.com/v1/security/oauth2/token";
    private const string LocationsUrl = "https://test.api.amadeus.com/v1/reference-data/locations";
    private const string FlightUrl = "https://test.api.amadeus.com/v2/shopping/flight-offers";

    public TravelCostExtractor(
        IRaythaDbContext db,
        IGenerativeAiService aiService,
        IConfiguration configuration)
    {
        _db = db;
        _aiService = aiService;
        _configuration = configuration;
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
        
        var targetTravelDate = DateTime.Parse(finalPackage.topic.close_date).AddMonths(4);

        // Fetch Amadeus API token
        string token = await GetAccessToken();

        // Resolve company HQ location
        string companyLocation = $"{finalPackage.company_city_hq}, {finalPackage.company_state_hq}";
        string originCode = await ResolveAirportCode(token, companyLocation);

        // Resolve government end-user location
        string governmentLocation = "Wright Patterson AFB, OH";
        string destinationCode = await ResolveAirportCode(token, governmentLocation);

        // Get flight cost
        decimal flightCost = await GetFlightCost(token, originCode, destinationCode, targetTravelDate);
        return flightCost.ToString();

        var travelCostResponse = await _aiService.GetStructuredResponse<FinalPackage.TravelCostInfo>(chatHistory, GetTravelCostJsonSchema());
        /*
        // Inject flight cost into the AI-generated response
        travelCostResponse.FlightCost = flightCost;

        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(travelCostResponse);
            JsonDocument.Parse(json); // Validate JSON
            return json;
        }
        catch (System.Text.Json.JsonException)
        {
            throw new Exception("Invalid JSON returned from travel cost calculation");
        }
        */
    }

    private async Task<string> GetAccessToken()
    {
        var amadeusApiKey = _configuration["Amadeus:ApiKey"] 
            ?? throw new InvalidOperationException("Amadeus API key not configured");
        var amadeusApiSecret = _configuration["Amadeus:ApiSecret"] 
            ?? throw new InvalidOperationException("Amadeus API secret not configured");

        using HttpClient client = new HttpClient();
        var formData = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", amadeusApiKey },
            { "client_secret", amadeusApiSecret }
        };

        var content = new FormUrlEncodedContent(formData);

        try 
        {
            HttpResponseMessage response = await client.PostAsync(TokenUrl, content);
            string responseContent = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Amadeus API error: {response.StatusCode} - {responseContent}");
            }

            JObject data = JObject.Parse(responseContent);
            return data["access_token"]?.ToString() ?? throw new Exception("Failed to retrieve access token.");
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to connect to Amadeus API: {ex.Message}");
        }
    }

    private async Task<string> ResolveAirportCode(string token, string cityOrLocation)
    {
        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var url = $"{LocationsUrl}?keyword={Uri.EscapeDataString(cityOrLocation)}&subType=AIRPORT&max=1";

        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();
        JObject data = JObject.Parse(json);

        var locationData = data["data"]?[0];
        if (locationData == null)
        {
            throw new Exception($"No airport found for location: {cityOrLocation}");
        }

        return locationData["iataCode"]?.ToString() ?? throw new Exception("IATA code not found for the location.");
    }

    private async Task<decimal> GetFlightCost(string token, string origin, string destination, DateTime travelDate)
    {
        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var url = $"{FlightUrl}?originLocationCode={origin}&destinationLocationCode={destination}&departureDate={travelDate:yyyy-MM-dd}&adults=1&travelClass=ECONOMY&nonStop=true&max=1";

        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();
        JObject data = JObject.Parse(json);

        var flightData = data["data"]?[0];
        if (flightData == null)
        {
            throw new Exception("No flight data found.");
        }

        string priceStr = flightData["price"]?["total"]?.ToString();
        return decimal.Parse(priceStr ?? "0");
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