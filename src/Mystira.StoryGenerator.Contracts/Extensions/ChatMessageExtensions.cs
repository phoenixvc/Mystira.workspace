using Mystira.StoryGenerator.Contracts.Chat;
using OpenAI.Chat;
using Google.Cloud.AIPlatform.V1;

namespace Mystira.StoryGenerator.Contracts.Extensions;

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
        // ensure that we append the system prompt
        var messages =
            request
                .Messages
                .Select(x => x.ToOpenAiChatMessage());
        if (request.SystemPrompt is not null)
            messages = messages.Append(ChatMessage.CreateSystemMessage(request.SystemPrompt));
        return messages;
    }

    public static Content ToGeminiContent(this MystiraChatMessage message)
    {
        var role = message.MessageType switch
        {
            ChatMessageType.AI => "model",
            ChatMessageType.System => "user", // Gemini treats system messages as user messages
            ChatMessageType.User => "user",
            _ => throw new ArgumentOutOfRangeException(nameof(message.MessageType), message.MessageType, null)
        };

        return new Content
        {
            Role = role,
            Parts = { new TextData { Text = message.Content } }
        };
    }

    public static IEnumerable<Content> ToGeminiContents(this ChatCompletionRequest request)
    {
        var contents = request.Messages.Select(x => x.ToGeminiContent());
        
        // Add system prompt as a user message if present (Gemini doesn't have separate system messages)
        if (request.SystemPrompt is not null)
        {
            contents = contents.Prepend(new Content
            {
                Role = "user",
                Parts = { new TextData { Text = $"System: {request.SystemPrompt}" } }
            });
        }
        
        return contents;
    }
}