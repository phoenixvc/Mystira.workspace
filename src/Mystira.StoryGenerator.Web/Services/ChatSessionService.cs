using System.Text.Json;
using Blazored.LocalStorage;
using Mystira.StoryGenerator.Contracts.Chat;

namespace Mystira.StoryGenerator.Web.Services;

public interface IChatSessionService
{
    ChatSession? CurrentSession { get; }
    IReadOnlyList<SessionMetadata> SessionHistory { get; }
    
    event Action<ChatSession?> CurrentSessionChanged;
    event Action<IReadOnlyList<SessionMetadata>> SessionHistoryChanged;
    
    Task<ChatSession> CreateNewSessionAsync(string? title = null);
    Task LoadSessionAsync(string sessionId);
    Task AddMessageAsync(MystiraChatMessage message);
    Task UpdateSessionTitleAsync(string sessionId, string newTitle);
    Task DeleteSessionAsync(string sessionId);
    Task SaveYamlSnapshotAsync(string yaml);
    Task UpdateProviderSettingsAsync(ProviderSettings settings);
    Task<List<SessionMetadata>> GetSessionHistoryAsync();
}

public class ChatSessionService : IChatSessionService
{
    private readonly ILocalStorageService _localStorage;
    
    private const string SessionsKey = "mystira_chat_sessions";
    private const string CurrentSessionKey = "mystira_current_session_id";
    
    private ChatSession? _currentSession;
    private List<SessionMetadata> _sessionHistory = new();

    public ChatSession? CurrentSession => _currentSession;
    public IReadOnlyList<SessionMetadata> SessionHistory => _sessionHistory;
    
    public event Action<ChatSession?>? CurrentSessionChanged;
    public event Action<IReadOnlyList<SessionMetadata>>? SessionHistoryChanged;

    public ChatSessionService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        // Load session history
        await LoadSessionHistoryAsync();
        
        // Try to restore last session
        var currentSessionId = await _localStorage.GetItemAsStringAsync(CurrentSessionKey);
        if (!string.IsNullOrEmpty(currentSessionId))
        {
            try
            {
                await LoadSessionAsync(currentSessionId);
            }
            catch
            {
                // If loading fails, clear the stored session ID
                await _localStorage.RemoveItemAsync(CurrentSessionKey);
            }
        }
    }

    public async Task<ChatSession> CreateNewSessionAsync(string? title = null)
    {
        var session = new ChatSession
        {
            Id = Guid.NewGuid().ToString(),
            Title = title ?? GenerateDefaultTitle(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Messages = new List<MystiraChatMessage>()
        };

        // Add system welcome message
        var welcomeMessage = new MystiraChatMessage
        {
            MessageType = ChatMessageType.System,
            Content = "Welcome to Mystira Story Generator! Let's create an amazing adventure together.",
            Timestamp = DateTime.UtcNow
        };
        session.Messages.Add(welcomeMessage);

        var themePromptMessage = new MystiraChatMessage
        {
            MessageType = ChatMessageType.System,
            Content = "To get started, share a short description or theme for your adventure so I can assemble the story parameters.",
            Timestamp = DateTime.UtcNow
        };
        session.Messages.Add(themePromptMessage);

        // Save session
        await SaveSessionAsync(session);
        
        // Update current session
        _currentSession = session;
        await _localStorage.SetItemAsStringAsync(CurrentSessionKey, session.Id);
        CurrentSessionChanged?.Invoke(_currentSession);

        // Update history
        await LoadSessionHistoryAsync();

        return session;
    }

    public async Task LoadSessionAsync(string sessionId)
    {
        var sessionKey = $"{SessionsKey}_{sessionId}";
        var sessionJson = await _localStorage.GetItemAsStringAsync(sessionKey);
        
        if (string.IsNullOrEmpty(sessionJson))
        {
            throw new InvalidOperationException($"Session {sessionId} not found.");
        }

        var session = JsonSerializer.Deserialize<ChatSession>(sessionJson);
        if (session == null)
        {
            throw new InvalidOperationException($"Failed to deserialize session {sessionId}.");
        }

        _currentSession = session;
        await _localStorage.SetItemAsStringAsync(CurrentSessionKey, session.Id);
        CurrentSessionChanged?.Invoke(_currentSession);
    }

    public async Task AddMessageAsync(MystiraChatMessage message)
    {
        if (_currentSession == null)
        {
            throw new InvalidOperationException("No active session. Create a new session first.");
        }

        _currentSession.Messages.Add(message);
        _currentSession.UpdatedAt = DateTime.UtcNow;

        await SaveSessionAsync(_currentSession);
        await LoadSessionHistoryAsync(); // Refresh history with updated timestamp
        
        CurrentSessionChanged?.Invoke(_currentSession);
    }

    public async Task UpdateSessionTitleAsync(string sessionId, string newTitle)
    {
        var sessionKey = $"{SessionsKey}_{sessionId}";
        var sessionJson = await _localStorage.GetItemAsStringAsync(sessionKey);
        
        if (string.IsNullOrEmpty(sessionJson))
        {
            return;
        }

        var session = JsonSerializer.Deserialize<ChatSession>(sessionJson);
        if (session != null)
        {
            session.Title = newTitle;
            session.UpdatedAt = DateTime.UtcNow;
            await SaveSessionAsync(session);

            // Update current session if it's the one being edited
            if (_currentSession?.Id == sessionId)
            {
                _currentSession.Title = newTitle;
                _currentSession.UpdatedAt = DateTime.UtcNow;
                CurrentSessionChanged?.Invoke(_currentSession);
            }

            await LoadSessionHistoryAsync();
        }
    }

    public async Task DeleteSessionAsync(string sessionId)
    {
        var sessionKey = $"{SessionsKey}_{sessionId}";
        await _localStorage.RemoveItemAsync(sessionKey);

        // If this was the current session, clear it
        if (_currentSession?.Id == sessionId)
        {
            _currentSession = null;
            await _localStorage.RemoveItemAsync(CurrentSessionKey);
            CurrentSessionChanged?.Invoke(null);
        }

        await LoadSessionHistoryAsync();
    }

    public async Task SaveYamlSnapshotAsync(string yaml)
    {
        if (_currentSession == null)
        {
            throw new InvalidOperationException("No active session.");
        }

        _currentSession.YamlSnapshot = yaml;
        _currentSession.UpdatedAt = DateTime.UtcNow;

        await SaveSessionAsync(_currentSession);
        await LoadSessionHistoryAsync(); // Refresh history
        
        CurrentSessionChanged?.Invoke(_currentSession);
    }

    public async Task UpdateProviderSettingsAsync(ProviderSettings settings)
    {
        if (_currentSession == null)
        {
            throw new InvalidOperationException("No active session.");
        }

        _currentSession.ProviderSettings = settings;
        _currentSession.UpdatedAt = DateTime.UtcNow;

        await SaveSessionAsync(_currentSession);
        CurrentSessionChanged?.Invoke(_currentSession);
    }

    public async Task<List<SessionMetadata>> GetSessionHistoryAsync()
    {
        await LoadSessionHistoryAsync();
        return _sessionHistory;
    }

    private async Task SaveSessionAsync(ChatSession session)
    {
        var sessionKey = $"{SessionsKey}_{session.Id}";
        var sessionJson = JsonSerializer.Serialize(session, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        await _localStorage.SetItemAsStringAsync(sessionKey, sessionJson);
    }

    private async Task LoadSessionHistoryAsync()
    {
        try
        {
            var keys = await _localStorage.KeysAsync();
            var sessionKeys = keys.Where(k => k.StartsWith(SessionsKey + "_")).ToList();

            var sessionList = new List<SessionMetadata>();

            foreach (var key in sessionKeys)
            {
                try
                {
                    var sessionJson = await _localStorage.GetItemAsStringAsync(key);
                    if (!string.IsNullOrEmpty(sessionJson))
                    {
                        var session = JsonSerializer.Deserialize<ChatSession>(sessionJson);
                        if (session != null)
                        {
                            sessionList.Add(new SessionMetadata
                            {
                                Id = session.Id,
                                Title = session.Title,
                                CreatedAt = session.CreatedAt,
                                UpdatedAt = session.UpdatedAt,
                                MessageCount = session.Messages?.Count ?? 0,
                                HasYamlSnapshot = !string.IsNullOrEmpty(session.YamlSnapshot)
                            });
                        }
                    }
                }
                catch
                {
                    // Skip corrupted sessions
                    continue;
                }
            }

            // Sort by updated date (most recent first)
            _sessionHistory = sessionList.OrderByDescending(s => s.UpdatedAt).ToList();
            SessionHistoryChanged?.Invoke(_sessionHistory);
        }
        catch
        {
            _sessionHistory = new List<SessionMetadata>();
            SessionHistoryChanged?.Invoke(_sessionHistory);
        }
    }

    private static string GenerateDefaultTitle()
    {
        var adjectives = new[] { "Epic", "Mysterious", "Magical", "Ancient", "Legendary", "Heroic", "Mystical", "Enchanted" };
        var nouns = new[] { "Quest", "Adventure", "Journey", "Tale", "Story", "Legend", "Chronicle", "Saga" };
        
        var random = new Random();
        var adjective = adjectives[random.Next(adjectives.Length)];
        var noun = nouns[random.Next(nouns.Length)];
        
        return $"{adjective} {noun}";
    }
}