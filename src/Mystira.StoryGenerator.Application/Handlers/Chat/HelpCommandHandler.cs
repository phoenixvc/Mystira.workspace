using System.Text;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Domain.Commands;
using Mystira.StoryGenerator.Domain.Commands.Chat;

namespace Mystira.StoryGenerator.Application.Handlers.Chat;

public class HelpCommandHandler : ICommandHandler<HelpCommand, ChatCompletionResponse>
{
    public Task<ChatCompletionResponse> Handle(HelpCommand request, CancellationToken cancellationToken)
    {
        var response = new ChatCompletionResponse
        {
            Provider = "system",
            Model = "help-template",
            Content = BuildHelpMessage(request.UserQuery)
        };

        return Task.FromResult(response);
    }

    private static string BuildHelpMessage(string? userQuery)
    {
        var sb = new StringBuilder();
        sb.AppendLine("👋 I'm Mystira's Story Generator, your branching-story co-author. I can:");
        sb.AppendLine();
        sb.AppendLine("Core story operations:");
        sb.AppendLine("• GenerateStory – create a brand-new JSON adventure that follows the Mystira schema");
        sb.AppendLine("• RefineStory – adjust tone, pacing, or details of an existing story");
        sb.AppendLine("• AutoFixStoryJson – repair malformed or invalid JSON so it matches the schema");
        sb.AppendLine("• ValidateStory – run schema and rules validation, reporting precise issues");
        sb.AppendLine("• SummarizeStory – produce an accessible TL;DR for caregivers or collaborators");
        sb.AppendLine();
        sb.AppendLine("Knowledge & support commands:");
        sb.AppendLine("• Help – show this overview anytime");
        sb.AppendLine("• SchemaDocs – explain fields, allowed values, and structure from the official schema");
        sb.AppendLine("• SafetyPolicy – clarify age-appropriateness and content limitations");
        sb.AppendLine("• Requirements – remind us what a complete Mystira story must include");
        sb.AppendLine("• Guidelines – share creative best practices and next-step suggestions");
        sb.AppendLine("• FreeText – ask anything else and I'll offer contextual tips");
        sb.AppendLine();
        sb.AppendLine("How to use me:");
        sb.AppendLine("1. Give natural language instructions: \"Create a cozy forest mystery for ages 6–9\".");
        sb.AppendLine("2. Paste JSON if you want validation, fixes, or refinements.");
        sb.AppendLine("3. Ask about schema, requirements, or safety for quick references.");
        sb.AppendLine("4. Keep iterating – I remember prior context within this chat session.");
        if (!string.IsNullOrWhiteSpace(userQuery))
        {
            sb.AppendLine();
            sb.AppendLine($"You asked: \"{userQuery.Trim()}\". Let me know which operation you want and I'll take it from there.");
        }

        return sb.ToString().TrimEnd();
    }
}
