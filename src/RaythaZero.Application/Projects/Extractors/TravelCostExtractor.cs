using Microsoft.SemanticKernel.ChatCompletion;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Domain.Entities;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Globalization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace RaythaZero.Application.Projects.Extractors;

public class TravelCostExtractor
{
    private readonly HttpClient _httpClient;
    private readonly string _amadeusApiKey;
    private readonly string _amadeusApiSecret;
    private readonly string _googleMapsApiKey;
    private readonly string _debugPath = "amadeus_debug.txt";

    public class AmadeusTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
    }

    public class GoogleGeocodeResponse
    {
        [JsonPropertyName("results")]
        public List<GoogleResult> Results { get; set; }
        
        public class GoogleResult
        {
            [JsonPropertyName("geometry")]
            public GoogleGeometry Geometry { get; set; }
        }
        
        public class GoogleGeometry
        {
            [JsonPropertyName("location")]
            public GoogleLocation Location { get; set; }
        }
        
        public class GoogleLocation
        {
            [JsonPropertyName("lat")]
            public double Lat { get; set; }
            [JsonPropertyName("lng")]
            public double Lng { get; set; }
        }
    }

    public class AmadeusAirportSearchResponse
    {
        [JsonPropertyName("data")]
        public List<AmadeusAirport> Data { get; set; }

        public class AmadeusAirport
        {
            [JsonPropertyName("iataCode")]
            public string IataCode { get; set; }
        }
    }

    public class AmadeusFlightSearchResponse
    {
        [JsonPropertyName("data")]
        public List<FlightOffer> Data { get; set; }

        public class FlightOffer
        {
            [JsonPropertyName("price")]
            public Price Price { get; set; }
        }

        public class Price
        {
            [JsonPropertyName("total")]
            public string Total { get; set; }
        }
    }

    public TravelCostExtractor(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _amadeusApiKey = configuration["Amadeus:ApiKey"];
        _amadeusApiSecret = configuration["Amadeus:ApiSecret"];
        _googleMapsApiKey = configuration["GoogleMaps:ApiKey"] ?? configuration["GoogleMapsApiKey"];

        // Initialize log file
        File.WriteAllText(_debugPath, ""); 

        // Log initial configuration state
        LogToFile($"Configuration loaded:");
        LogToFile($"Amadeus API Key exists: {!string.IsNullOrEmpty(_amadeusApiKey)}");
        LogToFile($"Amadeus API Secret exists: {!string.IsNullOrEmpty(_amadeusApiSecret)}");
        LogToFile($"Google Maps API Key exists: {!string.IsNullOrEmpty(_googleMapsApiKey)}");
    }

    public async Task<T> Extract<T>(FinalPackage finalPackage)
    {
        throw new NotImplementedException();
    }

    public async Task<string> Extract(FinalPackage package)
    {
        // Clear the debug file at the start of each extraction
        File.WriteAllText(_debugPath, "");
        LogToFile("Starting extraction, waiting 2 seconds before first API call...");
        
        await Task.Delay(2000);  // Add 2 second delay before first API call
        
        try
        {
            var token = await GetAccessToken();
            
            // Get origin airport code using company location
            var originLocation = $"{package.company_city_hq}, {package.company_state_hq}";
            var originAirport = await ResolveAirportCode(token, originLocation);

            // Get destination airport code using travel location
            var destinationLocation = $"{package.travel.end_user_location_city}, {package.travel.end_user_location_state}";
            var destinationAirport = await ResolveAirportCode(token, destinationLocation);

            // Search for flights using the airport codes
            var departureDate = DateTime.Now.AddMonths(6).ToString("yyyy-MM-dd");
            var returnDate = DateTime.Now.AddMonths(6).AddDays(4).ToString("yyyy-MM-dd");
            var flightPrice = await GetFlightPrice(token, originAirport, destinationAirport, departureDate, returnDate);

            var travelCostInfo = new FinalPackage.TravelCostInfo
            {
                FlightCost = flightPrice,
                LodgingRate = 0,
                MealRate = 0,
                NumberOfDays = 4,
                NumberOfNights = 3,
                TimePeriod = "Base Period"
            };

            return JsonSerializer.Serialize(travelCostInfo);
        }
        catch (Exception ex)
        {
            File.AppendAllText(_debugPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] Error: {ex.Message}\n");
            throw;  // Rethrow on any error
        }
    }

    private async Task<string> GetAccessToken()
    {
        LogToFile("Starting GetAccessToken method");
        
        if (string.IsNullOrEmpty(_amadeusApiKey))
        {
            var error = "Amadeus API key not configured. Please check configuration for Amadeus:ApiKey";
            LogToFile($"ERROR: {error}");
            throw new Exception(error);
        }

        if (string.IsNullOrEmpty(_amadeusApiSecret))
        {
            var error = "Amadeus API secret not configured. Please check configuration for Amadeus:ApiSecret";
            LogToFile($"ERROR: {error}");
            throw new Exception(error);
        }

        LogToFile("Amadeus API Key exists: True");
        LogToFile("Amadeus API Secret exists: True");
        LogToFile("Sending token request to Amadeus");

        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://test.api.amadeus.com/v1/security/oauth2/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"grant_type", "client_credentials"},
                {"client_id", _amadeusApiKey},
                {"client_secret", _amadeusApiSecret}
            })
        };

        var response = await _httpClient.SendAsync(tokenRequest);
        var content = await response.Content.ReadAsStringAsync();
        
        LogToFile($"Amadeus response status: {response.StatusCode}");
        LogToFile($"Amadeus response content: \n{content}\n");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get Amadeus access token: {response.StatusCode} - {content}");
        }

        var tokenResponse = JsonSerializer.Deserialize<AmadeusTokenResponse>(content);
        LogToFile("Successfully retrieved access token");
        
        return tokenResponse.AccessToken;
    }

    private async Task<(double Lat, double Lng)> GetCoordinates(string cityState)
    {
        LogToFile($"Getting coordinates for {cityState}");
        LogToFile($"Google Maps API Key value: {(_googleMapsApiKey?.Length > 0 ? _googleMapsApiKey[..5] + "..." : "null")}");
        
        if (string.IsNullOrEmpty(_googleMapsApiKey))
        {
            LogToFile($"ERROR: Google Maps API key not configured. Configuration paths checked:");
            LogToFile($"- Amadeus:GoogleMapsApiKey");
            LogToFile($"- GoogleMapsApiKey");
            throw new Exception("Google Maps API key not configured");
        }

        LogToFile("Google Maps API Key exists: True");

        var encodedLocation = Uri.EscapeDataString(cityState);
        var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={encodedLocation}&key={_googleMapsApiKey}";
        
        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        
        LogToFile($"Google Maps response status: {response.StatusCode}");
        LogToFile($"Google Maps response content: {content}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Google Maps API error: {response.StatusCode} - {content}");
        }

        var geocodeResponse = JsonSerializer.Deserialize<GoogleGeocodeResponse>(content);
        var location = geocodeResponse?.Results?.FirstOrDefault()?.Geometry?.Location;

        if (location == null)
        {
            throw new Exception($"No coordinates found for {cityState}");
        }

        LogToFile($"Found coordinates for {cityState}: {location.Lat}, {location.Lng}");
        return (Math.Round(location.Lat, 2), Math.Round(location.Lng, 2));
    }

    private async Task<string> ResolveAirportCode(string token, string location)
    {
        try
        {
            var coordinates = await GetCoordinates(location);
            LogToFile($"\nSearching for nearest airport to coordinates: {coordinates.Lat}, {coordinates.Lng}");

            var url = $"https://test.api.amadeus.com/v1/reference-data/locations/airports" +
                     $"?latitude={coordinates.Lat:0.00}" +
                     $"&longitude={coordinates.Lng:0.00}" +
                     $"&radius=100" +
                     $"&page[limit]=1";
            
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Bearer {token}");
            
            LogToFile($"Making Amadeus API call to: {url}");
            LogToFile($"With Authorization Bearer token: {token[..10]}... (truncated)");

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            LogToFile($"Amadeus Airport Search Response: {content}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Amadeus API error: {response.StatusCode} - {content}");
            }

            var searchResult = JsonSerializer.Deserialize<AmadeusAirportSearchResponse>(content);
            var airportCode = searchResult?.Data?.FirstOrDefault()?.IataCode;

            if (string.IsNullOrEmpty(airportCode))
            {
                throw new Exception($"No airport found near {location}");
            }

            LogToFile($"Found airport code {airportCode} for {location}");
            return airportCode;
        }
        catch (Exception ex)
        {
            LogToFile($"Error in ResolveAirportCode: {ex.Message}");
            throw;
        }
    }

    private async Task<decimal> GetFlightPrice(string token, string originAirport, string destinationAirport, string departureDate, string returnDate)
    {
        LogToFile($"\nSearching for round trip flights:");
        LogToFile($"From: {originAirport} to {destinationAirport}");
        LogToFile($"Outbound: {departureDate}");
        LogToFile($"Return: {returnDate}");

        var searchRequest = new
        {
            currencyCode = "USD",
            originDestinations = new[]
            {
                new
                {
                    id = "1",
                    originLocationCode = originAirport,
                    destinationLocationCode = destinationAirport,
                    departureDateTimeRange = new { date = departureDate }
                },
                new
                {
                    id = "2",
                    originLocationCode = destinationAirport,
                    destinationLocationCode = originAirport,
                    departureDateTimeRange = new { date = returnDate }
                }
            },
            travelers = new[]
            {
                new { id = "1", travelerType = "ADULT" }
            },
            sources = new[] { "GDS" },
            searchCriteria = new
            {
                maxFlightOffers = 1,
                flightFilters = new
                {
                    cabinRestrictions = new[]
                    {
                        new
                        {
                            cabin = "ECONOMY",
                            coverage = "MOST_SEGMENTS",
                            originDestinationIds = new[] { "1", "2" }
                        }
                    }
                }
            }
        };

        var url = "https://test.api.amadeus.com/v2/shopping/flight-offers";
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(searchRequest), System.Text.Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Authorization", $"Bearer {token}");

        LogToFile($"Request Body: {await request.Content.ReadAsStringAsync()}");

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        LogToFile($"Flight Search Response Status: {response.StatusCode}");
        LogToFile($"Flight Search Response: {content}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Flight search failed: {response.StatusCode} - {content}");
        }

        var searchResult = JsonSerializer.Deserialize<AmadeusFlightSearchResponse>(content);
        var price = searchResult?.Data?.FirstOrDefault()?.Price?.Total;

        if (string.IsNullOrEmpty(price))
        {
            throw new Exception("No flight price found");
        }

        if (!decimal.TryParse(price, out var flightPrice))
        {
            throw new Exception($"Invalid flight price format: {price}");
        }

        LogToFile($"Found round trip flight price: {flightPrice} USD");
        return flightPrice;
    }

    private void LogToFile(string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logMessage = $"[{timestamp}] {message}\n";
        File.AppendAllText(_debugPath, logMessage);
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