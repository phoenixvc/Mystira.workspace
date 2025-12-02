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

    Task EnsureInitializedAsync();
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
    private Task? _initializationTask;

    public ChatSession? CurrentSession => _currentSession;
    public IReadOnlyList<SessionMetadata> SessionHistory => _sessionHistory;

    public event Action<ChatSession?>? CurrentSessionChanged;
    public event Action<IReadOnlyList<SessionMetadata>>? SessionHistoryChanged;

    public ChatSessionService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
        _initializationTask = InitializeAsync();
    }

    /// <summary>
    /// Ensures the service has finished initial loading of session history and last session.
    /// </summary>
    public Task EnsureInitializedAsync() => _initializationTask ?? Task.CompletedTask;

    private async Task InitializeAsync()
    {
        // Load session history
        await LoadSessionHistoryAsync();

        // If no sessions found using the new storage scheme, attempt a one-time
        // migration from the legacy storage key used by older builds.
        if (_sessionHistory.Count == 0)
        {
            var migrated = await MigrateLegacyChatsAsync();
            if (migrated)
            {
                await LoadSessionHistoryAsync();
            }
        }

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
        else if (_sessionHistory.Count > 0)
        {
            // If we still don't have a current session id, pick the most recent session,
            // preferring one that has a YAML snapshot.
            var candidate = _sessionHistory
                .OrderByDescending(s => s.UpdatedAt)
                .ThenByDescending(s => s.HasYamlSnapshot)
                .FirstOrDefault();

            if (candidate != null)
            {
                try
                {
                    await LoadSessionAsync(candidate.Id);
                }
                catch
                {
                    // ignore; user can select another session manually
                }
            }
        }
    }

    /// <summary>
    /// Migrates chats stored in the legacy localStorage key "mystira_story_generator_chats"
    /// into the new per-session storage scheme used by <see cref="ChatSessionService"/>.
    /// Returns true if any sessions were migrated.
    /// </summary>
    private async Task<bool> MigrateLegacyChatsAsync()
    {
        try
        {
            const string legacyKey = "mystira_story_generator_chats";
            var legacyJson = await _localStorage.GetItemAsStringAsync(legacyKey);
            if (string.IsNullOrWhiteSpace(legacyJson))
                return false;

            using var doc = JsonDocument.Parse(legacyJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
                return false;

            var migratedAny = false;
            ChatSession? mostRecentWithSnapshot = null;
            DateTime mostRecentUpdatedAt = DateTime.MinValue;

            foreach (var item in doc.RootElement.EnumerateArray())
            {
                try
                {
                    var session = new ChatSession();

                    if (item.TryGetProperty("Id", out var idProp) && idProp.ValueKind == JsonValueKind.String)
                        session.Id = idProp.GetString() ?? session.Id;

                    if (item.TryGetProperty("Title", out var titleProp) && titleProp.ValueKind == JsonValueKind.String)
                        session.Title = titleProp.GetString() ?? session.Title;

                    if (item.TryGetProperty("CreatedAt", out var createdProp) && createdProp.ValueKind == JsonValueKind.String
                        && DateTime.TryParse(createdProp.GetString(), out var createdAt))
                        session.CreatedAt = createdAt;

                    // UpdatedAt might be missing – fall back to CreatedAt or now
                    if (item.TryGetProperty("UpdatedAt", out var updatedProp) && updatedProp.ValueKind == JsonValueKind.String
                        && DateTime.TryParse(updatedProp.GetString(), out var updatedAt))
                        session.UpdatedAt = updatedAt;
                    else
                        session.UpdatedAt = session.CreatedAt != default ? session.CreatedAt : DateTime.UtcNow;

                    if (item.TryGetProperty("YamlSnapshot", out var yamlProp) && yamlProp.ValueKind == JsonValueKind.String)
                        session.YamlSnapshot = yamlProp.GetString();

                    // Messages are optional and schema may differ; try a minimal best-effort mapping
                    if (item.TryGetProperty("Messages", out var msgsProp) && msgsProp.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var m in msgsProp.EnumerateArray())
                        {
                            try
                            {
                                // The legacy shape used Role/Content/Timestamp. Map to MystiraChatMessage.
                                string role = "user";
                                if (m.TryGetProperty("Role", out var roleProp) && roleProp.ValueKind == JsonValueKind.String)
                                    role = roleProp.GetString() ?? "user";

                                var msg = new MystiraChatMessage
                                {
                                    // If legacy role was "assistant", map to User for compatibility
                                    MessageType = role.Equals("system", StringComparison.OrdinalIgnoreCase)
                                        ? ChatMessageType.System
                                        : ChatMessageType.User,
                                    Content = (m.TryGetProperty("Content", out var contentProp) && contentProp.ValueKind == JsonValueKind.String)
                                        ? (contentProp.GetString() ?? string.Empty)
                                        : string.Empty,
                                    Timestamp = (m.TryGetProperty("Timestamp", out var tsProp) && tsProp.ValueKind == JsonValueKind.String
                                                 && DateTime.TryParse(tsProp.GetString(), out var ts))
                                        ? ts
                                        : DateTime.UtcNow
                                };

                                session.Messages.Add(msg);
                            }
                            catch
                            {
                                // Ignore malformed message entries
                            }
                        }
                    }

                    await SaveSessionAsync(session);
                    migratedAny = true;

                    // track most recent with snapshot
                    if (!string.IsNullOrWhiteSpace(session.YamlSnapshot) && session.UpdatedAt >= mostRecentUpdatedAt)
                    {
                        mostRecentUpdatedAt = session.UpdatedAt;
                        mostRecentWithSnapshot = session;
                    }
                }
                catch
                {
                    // Skip malformed legacy entries
                }
            }

            if (migratedAny)
            {
                // Set current session to the most recent with snapshot, or any most recent
                if (mostRecentWithSnapshot != null)
                {
                    await _localStorage.SetItemAsStringAsync(CurrentSessionKey, mostRecentWithSnapshot.Id);
                }
                else
                {
                    // As a fallback, pick the most recent session by UpdatedAt among all migrated ones
                    // Reload history to determine that
                    await LoadSessionHistoryAsync();
                    var candidate = _sessionHistory
                        .OrderByDescending(s => s.UpdatedAt)
                        .FirstOrDefault();
                    if (candidate != null)
                    {
                        await _localStorage.SetItemAsStringAsync(CurrentSessionKey, candidate.Id);
                    }
                }

                return true;
            }

            return false;
        }
        catch
        {
            return false;
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
