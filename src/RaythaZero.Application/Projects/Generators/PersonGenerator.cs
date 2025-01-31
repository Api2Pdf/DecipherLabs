using Microsoft.SemanticKernel.ChatCompletion;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Domain.Entities;
using System.Text;

namespace RaythaZero.Application.Projects.Extractors;

public class PersonGenerator : AbstractGenerator
{
    private readonly IRaythaDbContext _db;
    private readonly IGenerativeAiService _aiService;
    
    public PersonGenerator(
        IRaythaDbContext db,
        IGenerativeAiService aiService)
    {
        _db = db;
        _aiService = aiService;
    }
    
    public override async Task<string> Generate(FinalPackage finalPackage)
    {
        var result = new StringBuilder();
        
        foreach (var person in finalPackage.individuals)
        {
            finalPackage.current_person = person;
            
            // Generate intro and outro for this person
            var introPrompt = _db.Prompts.First(p => p.DeveloperName == "key_personnel_intro");
            var renderedIntroPrompt = ParsePrompt(introPrompt.PromptText, finalPackage);
            ChatHistory introChat = [];
            introChat.AddUserMessage(renderedIntroPrompt);
            var introResponse = (await _aiService.GetResponse(introChat)).First().Content;
            
            var outroPrompt = _db.Prompts.First(p => p.DeveloperName == "key_personnel_outro");
            var renderedOutroPrompt = ParsePrompt(outroPrompt.PromptText, finalPackage);
            ChatHistory outroChat = [];
            outroChat.AddUserMessage(renderedOutroPrompt);
            var outroResponse = (await _aiService.GetResponse(outroChat)).First().Content;
            
            // Add both for this person before moving to next person
            result.AppendLine(introResponse);
            result.AppendLine(outroResponse);
            result.AppendLine(); // Add blank line between people
        }
        
        return result.ToString();
    }

    public async Task<string> GenerateIntro(FinalPackage finalPackage)
    {
        try
        {
            File.AppendAllText("person_debug.txt", "\nStarting key personnel intro generation...\n");
            var introPrompt = _db.Prompts.First(p => p.DeveloperName == "key_personnel_intro");
            var allIntros = new StringBuilder();

            foreach (var individual in finalPackage.individuals)
            {
                finalPackage.current_person = individual;  // Set current person
                
                var renderedIntroPrompt = ParsePrompt(introPrompt.PromptText, finalPackage);
                File.AppendAllText("person_debug.txt", $"Generating intro for {individual.name}...\n");
                
                ChatHistory introChat = [];
                introChat.AddUserMessage(renderedIntroPrompt);
                var response = (await _aiService.GetResponse(introChat)).First().Content;
                File.AppendAllText("person_debug.txt", $"Generated intro for {individual.name}\n");
                
                allIntros.AppendLine(response);
            }

            return allIntros.ToString();
        }
        catch (Exception ex)
        {
            File.AppendAllText("person_debug.txt", $"Error in GenerateIntro: {ex.Message}\n{ex.StackTrace}\n");
            throw;
        }
    }

    public async Task<string> GenerateOutro(FinalPackage finalPackage)
    {
        try
        {
            File.AppendAllText("person_debug.txt", "\nStarting key personnel outro generation...\n");
            var outroPrompt = _db.Prompts.First(p => p.DeveloperName == "key_personnel_outro");
            var allOutros = new StringBuilder();

            foreach (var individual in finalPackage.individuals)
            {
                finalPackage.current_person = individual;  // Set current person
                
                var renderedOutroPrompt = ParsePrompt(outroPrompt.PromptText, finalPackage);
                File.AppendAllText("person_debug.txt", $"Generating outro for {individual.name}...\n");
                
                ChatHistory outroChat = [];
                outroChat.AddUserMessage(renderedOutroPrompt);
                var response = (await _aiService.GetResponse(outroChat)).First().Content;
                File.AppendAllText("person_debug.txt", $"Generated outro for {individual.name}\n");
                
                allOutros.AppendLine(response);
            }

            return allOutros.ToString();
        }
        catch (Exception ex)
        {
            File.AppendAllText("person_debug.txt", $"Error in GenerateOutro: {ex.Message}\n{ex.StackTrace}\n");
            throw;
        }
    }
}