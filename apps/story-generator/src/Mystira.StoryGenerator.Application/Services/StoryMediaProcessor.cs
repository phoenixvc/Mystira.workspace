using System.Text.Json;
using System.Text.Json.Nodes;

namespace Mystira.StoryGenerator.Application.Services;

public class StoryMediaProcessor : IStoryMediaProcessor
{
    public string ProcessMediaIds(string storyJson)
    {
        if (string.IsNullOrWhiteSpace(storyJson))
            return storyJson;

        try
        {
            var root = JsonNode.Parse(storyJson);
            if (root is not JsonObject rootObj) return storyJson;

            var scenesArray = rootObj["scenes"] as JsonArray;
            if (scenesArray == null || scenesArray.Count == 0) return storyJson;

            // 1. Build the scene graph and metadata
            var scenes = new Dictionary<string, SceneMetadata>();
            var sceneList = new List<string>(); // to preserve order if needed, but BFS is better for depth

            foreach (var sceneNode in scenesArray)
            {
                if (sceneNode is not JsonObject s) continue;
                var id = s["id"]?.ToString();
                if (string.IsNullOrEmpty(id)) continue;

                var metadata = new SceneMetadata { Id = id, Node = s };

                // Get transitions
                var nextScene = s["next_scene"]?.ToString();
                if (!string.IsNullOrEmpty(nextScene))
                {
                    metadata.OutgoingLinks.Add(nextScene);
                }

                var branches = s["branches"] as JsonArray;
                if (branches != null)
                {
                    foreach (var branch in branches)
                    {
                        var target = branch?["next_scene"]?.ToString();
                        if (!string.IsNullOrEmpty(target))
                        {
                            metadata.OutgoingLinks.Add(target);
                        }
                    }
                }

                scenes[id] = metadata;
                sceneList.Add(id);
            }

            if (scenes.Count == 0) return storyJson;

            // 2. Calculate depths and choice letters using BFS
            // Assuming the first scene in the array is the start scene
            var startSceneId = sceneList[0];
            var queue = new Queue<(string Id, int Depth)>();
            queue.Enqueue((startSceneId, 1));

            var visited = new HashSet<string>();
            var depthMap = new Dictionary<int, List<string>>();

            while (queue.Count > 0)
            {
                var (currentId, depth) = queue.Dequeue();
                if (visited.Contains(currentId)) continue;
                visited.Add(currentId);

                if (!scenes.TryGetValue(currentId, out var metadata)) continue;
                metadata.Depth = depth;

                if (!depthMap.ContainsKey(depth)) depthMap[depth] = new List<string>();
                depthMap[depth].Add(currentId);

                foreach (var neighbor in metadata.OutgoingLinks)
                {
                    if (!visited.Contains(neighbor))
                    {
                        queue.Enqueue((neighbor, depth + 1));
                    }
                }
            }

            // Handle orphan scenes if any (unreachable from start)
            foreach (var sceneId in sceneList)
            {
                if (!visited.Contains(sceneId))
                {
                    scenes[sceneId].Depth = -1; // Unknown depth
                }
            }

            // 3. Assign choice identifiers (a, b, c...) based on position in depthMap
            foreach (var depth in depthMap.Keys.OrderBy(d => d))
            {
                var scenesAtDepth = depthMap[depth];
                for (int i = 0; i < scenesAtDepth.Count; i++)
                {
                    var choiceChar = (char)('a' + i);
                    scenes[scenesAtDepth[i]].ChoiceId = choiceChar.ToString();
                }
            }

            // 4. Update media IDs in the JSON
            foreach (var metadata in scenes.Values)
            {
                if (metadata.Depth <= 0) continue;

                var media = metadata.Node["media"] as JsonObject;
                if (media == null) continue;

                UpdateMediaField(media, "image", metadata, ".webp");
                UpdateMediaField(media, "audio", metadata, ".mp3");
                UpdateMediaField(media, "video", metadata, ".mp3");
            }

            return root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            // Fallback to original if something goes wrong during processing
            return storyJson;
        }
    }

    private void UpdateMediaField(JsonObject media, string key, SceneMetadata metadata, string extension)
    {
        var currentVal = media[key]?.ToString();
        if (string.IsNullOrEmpty(currentVal)) return;

        // format: [number][choice]_[scene_id].[extension]
        var newId = $"{metadata.Depth}{metadata.ChoiceId}_{metadata.Id}{extension}";
        media[key] = JsonValue.Create(newId);
    }

    private class SceneMetadata
    {
        public string Id { get; set; } = string.Empty;
        public JsonObject Node { get; set; } = null!;
        public List<string> OutgoingLinks { get; } = new();
        public int Depth { get; set; }
        public string ChoiceId { get; set; } = string.Empty;
    }
}
