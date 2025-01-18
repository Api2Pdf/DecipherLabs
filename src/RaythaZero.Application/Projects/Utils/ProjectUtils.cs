using System.Text.Json;
using RaythaZero.Application.Common.Utils;
using RaythaZero.Domain.Entities;

namespace RaythaZero.Application.Projects.Utils;

public static class ProjectUtils
{
    public static IEnumerable<Topic> GetTopics()
    {
        var topicsAsJson = StringExtensions.ReadJsonFile("RaythaZero.Application.Projects.Utils.topics.json");
        var topics = JsonSerializer.Deserialize<IEnumerable<Topic>>(topicsAsJson);
        
        return topics ?? Enumerable.Empty<Topic>();
    }
}