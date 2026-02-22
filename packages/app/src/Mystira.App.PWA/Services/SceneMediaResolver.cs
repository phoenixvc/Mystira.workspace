using Mystira.App.PWA.Models;

namespace Mystira.App.PWA.Services;

/// <summary>
/// Extension methods for resolving scene media URLs via the API client.
/// Eliminates the repeated 3-line null-check-and-resolve pattern.
/// </summary>
public static class SceneMediaResolver
{
    /// <summary>
    /// Resolves AudioUrl, ImageUrl, and VideoUrl from the scene's Media references.
    /// </summary>
    public static async Task ResolveMediaUrlsAsync(this Scene scene, IApiClient apiClient)
    {
        if (scene.Media == null)
            return;

        scene.AudioUrl = !string.IsNullOrEmpty(scene.Media.Audio)
            ? await apiClient.GetMediaUrlFromId(scene.Media.Audio) : null;
        scene.ImageUrl = !string.IsNullOrEmpty(scene.Media.Image)
            ? await apiClient.GetMediaUrlFromId(scene.Media.Image) : null;
        scene.VideoUrl = !string.IsNullOrEmpty(scene.Media.Video)
            ? await apiClient.GetMediaUrlFromId(scene.Media.Video) : null;
    }
}
