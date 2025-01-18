using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace RaythaZero.Application.Common.Interfaces;

public interface IGenerativeAiService
{
    Task<IReadOnlyList<ChatMessageContent>> GetResponse(ChatHistory history);
}
