using Mystira.StoryGenerator.Contracts.Chat;
using OpenAI.Chat;

namespace Mystira.StoryGenerator.Llm.Extensions;

public static class ChatMessageExtensions
{
    public static ChatMessage ToOpenAiChatMessage(this MystiraChatMessage message)
    {
        ChatMessage chatMessage = message.MessageType switch
        {
            ChatMessageType.AI => ChatMessage.CreateAssistantMessage(message.Content),
            ChatMessageType.System => ChatMessage.CreateSystemMessage(message.Content),
            ChatMessageType.User => ChatMessage.CreateUserMessage(message.Content),
            _ => throw new ArgumentOutOfRangeException(nameof(message.MessageType), message.MessageType, null)
        };

        return chatMessage;
    }

    public static IEnumerable<ChatMessage> ToOpenAiChatMessages(this ChatCompletionRequest request)
    {
        var messages = request.Messages.Select(x => x.ToOpenAiChatMessage());
        if (request.SystemPrompt is not null)
            messages = messages.Append(ChatMessage.CreateSystemMessage(request.SystemPrompt));
        return messages;
    }
}
