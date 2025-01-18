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
}