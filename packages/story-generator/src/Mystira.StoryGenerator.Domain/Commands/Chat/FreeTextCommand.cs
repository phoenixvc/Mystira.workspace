using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Domain.Commands.Chat;

public class FreeTextCommand : ICommand<ChatCompletionResponse>
{
    public FreeTextCommand(ChatContext context, string? intent, IEnumerable<MystiraChatMessage>? history = null)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Intent = intent;
        History = history ?? Enumerable.Empty<MystiraChatMessage>();
    }

    public ChatContext Context { get; }
    public string? Intent { get; }
    public IEnumerable<MystiraChatMessage> History { get; }
}
