// See https://aka.ms/new-console-template for more information

using System.ClientModel;
using Azure.AI.OpenAI;
using OpenAI.Chat;

AzureOpenAIClient azureClient = new(
    new Uri("https://dev-swe-ai-mystira-stor-resource.openai.azure.com/"),
    new ApiKeyCredential("KEY"));
ChatClient chatClient = azureClient.GetChatClient("gpt-5-nano");
#pragma warning disable OPENAI001
Console.WriteLine(chatClient.Model);
#pragma warning restore OPENAI001
var msg = "user: Hello World!";
Console.WriteLine(msg);
var response = await chatClient.CompleteChatAsync(new ChatMessage[] {ChatMessage.CreateUserMessage(msg)});
Console.WriteLine($"AI: {response.Value.Content[0].Text}");
//https://dev-swe-ai-mystira-stor-resource.services.ai.azure.com/api/projects/dev-swe-ai-mystira-storygen
