using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Mystira.StoryGenerator.Contracts.Entities;
using Mystira.StoryGenerator.Llm.Services.DominatorBasedConsistency;

namespace Mystira.StoryGenerator.Llm.Console.Tests;

internal static class EventClassificationConsoleTests
{
    public static async Task<int> RunAsync(IServiceProvider services, ILogger logger)
    {
        var classifier = services.GetRequiredService<SceneEntityLlmClassifier>();

        // Event-centric sample scenes with expected entities (strict: name, type, isProper)
        var tests = new Dictionary<string, SceneEntity[]> {
            {
                "Alice steps into the Grand Market, clutching the Silver Key.",
                [
                    new SceneEntity { Name = "Alice", Type = SceneEntityType.Character, IsProperNoun = true, Confidence = Confidence.High},
                    new SceneEntity { Name = "Grand Market", Type = SceneEntityType.Location, IsProperNoun = true, Confidence = Confidence.High },
                    new SceneEntity { Name = "Silver Key", Type = SceneEntityType.Item, IsProperNoun = true, Confidence = Confidence.High }
                ]
            },
            {
                "At the Tower of Dawn, Captain Reyes consults the Codex of Storms.",
                [
                    new SceneEntity { Name = "Tower of Dawn", Type = SceneEntityType.Location, IsProperNoun = true, Confidence = Confidence.High },
                    new SceneEntity { Name = "Captain Reyes", Type = SceneEntityType.Character, IsProperNoun = true, Confidence = Confidence.High },
                    new SceneEntity { Name = "Codex of Storms", Type = SceneEntityType.Item, IsProperNoun = true, Confidence = Confidence.High }
                ]
            },
            {
                "The Shadow Guild extends its reach into Rivermoor.",
                [
                    new SceneEntity { Name = "Shadow Guild", Type = SceneEntityType.Concept, IsProperNoun = true, Confidence = Confidence.High },
                    new SceneEntity { Name = "Rivermoor", Type = SceneEntityType.Location, IsProperNoun = true, Confidence = Confidence.High }
                ]
            },
            {
                "Beneath the Crystal Bridge, Mira whispers an ancient promise to the river.",
                [
                    new SceneEntity { Name = "Crystal Bridge", Type = SceneEntityType.Location, IsProperNoun = true, Confidence = Confidence.High },
                    new SceneEntity { Name = "Mira", Type = SceneEntityType.Character, IsProperNoun = true, Confidence = Confidence.High },
                ]
            },
            {
                "The Festival of Fallen Stars begins as lanterns bloom above Emberfall.",
                [
                    // Named event as a concept
                    new SceneEntity { Name = "Festival of Fallen Stars", Type = SceneEntityType.Concept, IsProperNoun = true, Confidence = Confidence.High },
                    new SceneEntity { Name = "Emberfall", Type = SceneEntityType.Location, IsProperNoun = true, Confidence = Confidence.High }
                ]
            },
            {
                "Jonas tightens his grip on the Emberknife and scans the Hollow Road.",
                [
                    new SceneEntity { Name = "Jonas", Type = SceneEntityType.Character, IsProperNoun = true, Confidence = Confidence.High },
                    new SceneEntity { Name = "Emberknife", Type = SceneEntityType.Item, IsProperNoun = true, Confidence = Confidence.High },
                    new SceneEntity { Name = "Hollow Road", Type = SceneEntityType.Location, IsProperNoun = true, Confidence = Confidence.High }
                ]
            },
            {
                "Fear and curiosity wrestle inside Liora as she approaches the Gate of Whispers.",
                [
                    // Pure abstract concepts, not proper nouns
                    new SceneEntity { Name = "Fear", Type = SceneEntityType.Concept, IsProperNoun = false, Confidence = Confidence.Medium },
                    new SceneEntity { Name = "Curiosity", Type = SceneEntityType.Concept, IsProperNoun = false, Confidence = Confidence.Medium },
                    new SceneEntity { Name = "Liora", Type = SceneEntityType.Character, IsProperNoun = true, Confidence = Confidence.High },
                    new SceneEntity { Name = "Gate of Whispers", Type = SceneEntityType.Location, IsProperNoun = true, Confidence = Confidence.High }
                ]
            },
            {
                "In the Library of Echoes, every broken promise leaves a silver scar on the floor.",
                [
                    new SceneEntity { Name = "Library of Echoes", Type = SceneEntityType.Location, IsProperNoun = true, Confidence = Confidence.High },
                    // Abstract, non-proper concept
                    new SceneEntity { Name = "broken promise", Type = SceneEntityType.Concept, IsProperNoun = false, Confidence = Confidence.Low },
                ]
            },
            {
                "Professor Elan records the Oath of Greenfire in his journal.",
                [
                    new SceneEntity { Name = "Professor Elan", Type = SceneEntityType.Character, IsProperNoun = true, Confidence = Confidence.High },
                    // Named oath as a concept
                    new SceneEntity { Name = "Oath of Greenfire", Type = SceneEntityType.Concept, IsProperNoun = true, Confidence = Confidence.High }
                ]
            },
            {
                "Rain patters softly against the window until the candles finally go out.",
                []
            }
        };

        // Run tests in parallel with a reasonable degree of parallelism
        var tasks = new List<Task<(bool pass, string scene, string message)>>();
        foreach (var kvp in tests)
        {
            var scene = kvp.Key;
            var expected = kvp.Value;
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var result = await classifier.ClassifyAsync(scene);
                    var actual = result?.Entities ?? [];

                    // If classifier returned no entities, this is only a failure when we expected some.
                    if (actual.Length == 0)
                    {
                        if (expected.Length == 0)
                        {
                            return (true, scene, "PASS: No entities expected and none returned.");
                        }
                        return (false, scene, "No entities returned by classifier, but some were expected.");
                    }

                    // Strict comparison per expected entity: match by name (case-insensitive), then verify type and proper noun
                    var issues = new List<string>();

                    foreach (var exp in expected)
                    {
                        var match = actual.FirstOrDefault(a => string.Equals(a.Name, exp.Name, StringComparison.OrdinalIgnoreCase));
                        if (match == null)
                        {
                            issues.Add($"Missing entity: name='{exp.Name}', type={exp.Type}, isProper={exp.IsProperNoun}, confidence={exp.Confidence}");
                            continue;
                        }

                        var typeOk = match.Type == exp.Type;
                        var properOk = match.IsProperNoun == exp.IsProperNoun;
                        if (!typeOk || !properOk)
                        {
                            var diffs = new List<string>();
                            if (!typeOk) diffs.Add($"type expected={exp.Type} actual={match.Type}");
                            if (!properOk) diffs.Add($"is_proper_noun expected={exp.IsProperNoun} actual={match.IsProperNoun}");
                            issues.Add($"Mismatch for '{exp.Name}': {string.Join(", ", diffs)}");
                        }
                    }

                    // Optionally, report unexpected extras (not a failure per requirements, but informative)
                    var extras = actual.Where(a => !expected.Any(e => string.Equals(e.Name, a.Name, StringComparison.OrdinalIgnoreCase))).ToList();
                    if (extras.Count > 0)
                    {
                        var extrasStr = string.Join(", ", extras.Select(e => $"{e.Name} ({e.Type}, proper={e.IsProperNoun}, confidence={e.Confidence})"));
                        issues.Add($"Note: unexpected entities returned: {extrasStr}");
                    }

                    if (issues.Count == 0)
                    {
                        var reported = string.Join(", ", actual.Select(a => $"{a.Name} ({a.Type}, proper={a.IsProperNoun}, confidence={a.Confidence})"));
                        return (true, scene, $"PASS: All expected entities matched on name, type, and proper noun. Reported: {reported}");
                    }
                    else
                    {
                        var reported = string.Join(", ", actual.Select(a => $"{a.Name} ({a.Type}, proper={a.IsProperNoun}, confidence={a.Confidence})"));
                        return (false, scene, $"FAIL: {string.Join("; ", issues)}. Model returned: {reported}");
                    }
                }
                catch (Exception ex)
                {
                    return (false, scene, $"Exception: {ex.Message}");
                }
            }));
        }

        var results = await Task.WhenAll(tasks);
        int passed = 0, failed = 0, index = 1;
        foreach (var r in results)
        {
            if (r.pass)
            {
                logger.LogInformation("Test {Index}: {Scene}\n  {Message}", index, r.scene, r.message);
                passed++;
            }
            else
            {
                logger.LogError("Test {Index}: {Scene}\n  {Message}", index, r.scene, r.message);
                failed++;
            }
            index++;
        }

        logger.LogInformation("Summary: {Passed} passed, {Failed} failed, {Total} total", passed, failed, passed + failed);
        return failed == 0 ? 0 : 1;
    }
}
