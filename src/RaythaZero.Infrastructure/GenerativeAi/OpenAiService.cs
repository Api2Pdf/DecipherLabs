using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using RaythaZero.Application.Common.Interfaces;

namespace RaythaZero.Infrastructure.GenerativeAi;

public record OpenAiSettings
{
    public string OPENAI_MODEL_ID { get; set; }
    public string OPENAI_API_KEY{ get; set; } 
}

public class OpenAiService : IGenerativeAiService
{
    private readonly OpenAiSettings _settings;

    public OpenAiService(IOptions<OpenAiSettings> options)
    {
        _settings = options.Value;
    }

    public async Task<IReadOnlyList<ChatMessageContent>> GetResponse(ChatHistory history)
    {
        OpenAIChatCompletionService chatCompletionService = new (
            modelId: _settings.OPENAI_MODEL_ID,
            apiKey: _settings.OPENAI_API_KEY
        );
        var response = await chatCompletionService.GetChatMessageContentsAsync(history);
        return response;
    }
    
    public async Task<T> GetStructuredResponse<T>(ChatHistory history)
    {
        OpenAIChatCompletionService chatCompletionService = new (
            modelId: _settings.OPENAI_MODEL_ID,
            apiKey: _settings.OPENAI_API_KEY
        );

        var executionSettings = new OpenAIPromptExecutionSettings()
        {
            ResponseFormat = typeof(T),
            
        };
        var response = await chatCompletionService.GetChatMessageContentsAsync(history, executionSettings);
        var result = JsonSerializer.Deserialize<T>(response.First()?.Content ?? string.Empty);
        return result;
    }
    
    public async Task<T> GetStructuredResponse<T>(ChatHistory history, string jsonSchema)
    {
        OpenAIChatCompletionService chatCompletionService = new (
            modelId: _settings.OPENAI_MODEL_ID,
            apiKey: _settings.OPENAI_API_KEY
        );
        ChatResponseFormat chatResponseFormat = ChatResponseFormat.ForJsonSchema(jsonSchema);
        var executionSettings = new OpenAIPromptExecutionSettings()
        {
            ResponseFormat = chatResponseFormat
        };
        var response = await chatCompletionService.GetChatMessageContentsAsync(history, executionSettings);
        var result = JsonSerializer.Deserialize<T>(response.First()?.Content ?? string.Empty);
        return result;
    }
}