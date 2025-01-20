using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace RaythaZero.Application.Common.Interfaces;

public interface IGenerativeAiService
{
    Task<IReadOnlyList<ChatMessageContent>> GetResponse(ChatHistory history);
    Task<T> GetStructuredResponse<T>(ChatHistory history);
    Task<T> GetStructuredResponse<T>(ChatHistory history, string jsonSchema);
}
