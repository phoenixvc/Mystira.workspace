using System.Text;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Application.Handlers.Chat;

internal static class ChatContextSummaryBuilder
{
    public static string BuildContextSummary(ChatContext context, int storyCharacterLimit = 10000, int recentUserMessages = 3)
    {
        if (context == null)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();

        if (context.CurrentStory is { Content.Length: > 0 })
        {
            var content = context.CurrentStory.Content;
            if (content.Length > storyCharacterLimit)
            {
                content = content[..storyCharacterLimit] + "...";
            }

            sb.AppendLine("Current story snapshot (truncated):");
            sb.AppendLine(content);
            sb.AppendLine();
        }

        var userMessages = (context.Messages ?? new List<MystiraChatMessage>())
            .Where(message => message.MessageType == ChatMessageType.User)
            .TakeLast(recentUserMessages)
            .Select(message => message.Content)
            .Where(content => !string.IsNullOrWhiteSpace(content))
            .ToList();

        if (userMessages.Count > 0)
        {
            sb.AppendLine("Recent user prompts or goals:");
            foreach (var message in userMessages)
            {
                sb.AppendLine($"- {message!.Trim()}");
            }
        }

        return sb.ToString().Trim();
    }
}
