using Fluid;
using RaythaZero.Domain.Entities;

namespace RaythaZero.Application.Projects.Extractors;

public abstract class AbstractExtractor
{
    public abstract Task<T> Extract<T>(FinalPackage package);
    public abstract Task<string> Extract(FinalPackage package);
    
    protected string ParsePrompt(string source, FinalPackage model)
    {
        var parser = new FluidParser();

        if (parser.TryParse(source, out var template, out var error))
        {
            var options = new TemplateOptions();
            options.MemberAccessStrategy = new UnsafeMemberAccessStrategy();
            var context = new TemplateContext(model, options);
            return template.Render(context);
        }
        else
        {
            throw new Exception($"Failed to parse template: {error}");
        }
    }
}