using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Domain.Commands;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Domain.Commands.Chat;

public class FreeTextCommand : ICommand<ChatCompletionResponse>
{
    public FreeTextCommand(ChatContext context, string? intent)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Intent = intent;
    }

    public ChatContext Context { get; }
    public string? Intent { get; }
}
