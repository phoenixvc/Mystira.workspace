using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Llm.Services.DominatorBasedConsistency;

namespace Mystira.StoryGenerator.Llm.Console.Tests;

internal static class ConsistencyConsoleTests
{
    public static async Task<int> RunAsync(IServiceProvider services, ILogger logger)
    {
        var evaluator = services.GetRequiredService<ScenarioConsistencyLlmEvaluator>();

        // Test inputs as specified in the issue description
        var cases = new List<(string name, string content, bool expectOk)> {
            ("No Errors",
@"Scene scene_start: Luna the fox trotted through Mossy Meadow. She noticed a tiny blue mushroom glowing softly.
Scene scene_choice_investigate: Luna feels curious. Do you sniff the mushroom or touch it gently?
Answer: Sniff the mushroom.
Scene scene_sniff_result: Luna sniffed the mushroom and discovered it smelled like fresh rain.
Scene ending_good: Feeling peaceful, she padded home with a smile.",
            true),

            ("Error - character without intro",
@"Scene scene_start: Milo walked along the riverbank, collecting shiny stones.
Scene scene_next: Suddenly, Zara jumped out and said, ""Hello!""
Scene scene_previous_reference: ""Just like earlier today,"" Zara added, though she hadn't appeared before.
Scene ending: Milo nodded politely and walked home.",
            false),

            ("Error - unintroduced item",
@"Scene scene_start: Tilly the hedgehog wandered through Bright Hollow.
Scene scene_confusion: ""Where did the magic lantern go?"" she asked.
Scene scene_later: She finally found the lantern tucked under a log.
Scene ending: Tilly smiled in relief.",
            false),

            ("Error - entity disappears and then reappears",
@"Scene scene_start: Bruno the bear walked with Hazel the squirrel through Tallgrass Field.
Scene scene_split: Hazel waved goodbye and ran home.
Scene scene_sudden: ""Let's go together!"" Hazel suddenly said, standing next to Bruno again.
Scene ending: They continued on their way.",
            false),

            ("Error - temporal / time-flow errors",
@"Scene scene_morning: It was early morning as Fern the owl stretched her wings.
Scene scene_midday: ""It's nearly lunchtime,"" she hooted.
Scene scene_instant_night: Suddenly, the forest was pitch-black night.
Scene ending: Fern blinked in confusion.",
            false),

            ("Error - emotional / behavioral inconsistency (hide then brave)",
@"Scene scene_start: Nia trembled at the sight of the dark cave.
Scene scene_choice_approach: Should Nia hide or enter bravely?
Answer: Hide behind the tree.
Scene scene_next: Nia ran straight into the cave shouting, ""I'm not scared at all!""
Scene ending: The echo answered cheerfully.",
            false),

            ("Error - emotional / behavioral inconsistency (friendship sudden)",
@"Scene scene_start: Lark the bird cried because Rumble the raccoon stole her berries.
Scene scene_choice: Should she ask him or ignore him?
Answer: Ask him.
Scene scene_response: Rumble laughed and said he would do it again.
Scene scene_next: ""You're my best friend!"" Lark chirped happily.
Scene ending: They flew away together.",
            false),

            ("Error - causal logic",
@"Scene scene_start: Briar the bunny found a locked wooden box.
Scene scene_choice: Do you leave the box alone or open it gently?
Answer: Leave the box alone.
Scene scene_result: Briar opened the box and gasped at the treasure.
Scene ending: She hopped away excitedly.",
            false),

            ("Error - unsupported deduction",
@"Scene scene_start: Dusty the raccoon saw footprints in the mud.
Scene scene_thought: ""They must belong to a dragon,"" Dusty declared.
Scene ending: He nodded confidently, though nothing indicated dragons exist here.",
            false),

            ("Error - world inconsistency rule",
@"Scene scene_start: The glowing pebble could only show faint light, nothing more.
Scene scene_action: ""Help us fly!"" yelled Kora.
Scene scene_result: The pebble lifted them into the sky.
Scene ending: They soared above the trees.",
            false),

            ("Error - time + emotional + entity",
@"Scene scene_morning: It was dawn, and Mira the mouse felt too scared to leave her burrow.
Scene scene_midday_sudden: Without explanation, it was suddenly sunset.
Scene scene_fear_switch: Mira marched out confidently, laughing loudly.
Scene scene_new_char: ""I'm Rowan,"" said a wolf she had never met. ""We've been friends forever!""
Scene ending: Mira nodded cheerfully.",
            false)
        };

        int passed = 0, failed = 0;

        // Run in parallel with bounded degree of parallelism
        var degree = Math.Max(2, Math.Min(Environment.ProcessorCount, 8));
        using var gate = new SemaphoreSlim(degree, degree);

        var tasks = new List<Task>();
        for (int i = 0; i < cases.Count; i++)
        {
            var caseIndex = i + 1;
            var (name, content, expectOk) = cases[i];

            await gate.WaitAsync();
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    // Evaluate
                    var result = await evaluator.EvaluateConsistencyAsync(content);

                    // Pretty print the LLM output (typed result). If null, note it.
                    string llmJson;
                    if (result == null)
                    {
                        llmJson = "<no result>";
                        logger.LogWarning("[{Index}] {Name}: Evaluator not configured or response not parseable.", caseIndex, name);
                    }
                    else
                    {
                        llmJson = System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                    }

                    // Output scenario text and LLM output as separate paragraphs
                    logger.LogInformation("\n[{Index}] {Name} :: Scenario\n{Scenario}\n\n[{Index}] {Name} :: LLM Output (JSON)\n{Json}\n", caseIndex, name, content, caseIndex, name, llmJson);

                    if (result != null)
                    {
                        bool isOk = string.Equals(result.OverallAssessment, "ok", StringComparison.OrdinalIgnoreCase) || (result.Issues?.Count ?? 0) == 0;
                        bool pass = expectOk ? isOk : !isOk;

                        if (pass)
                        {
                            logger.LogInformation("[{Index}] {Name}: PASS. Assessment={Assessment}, Issues={IssueCount}", caseIndex, name, result.OverallAssessment, result.Issues?.Count ?? 0);
                            if ((result.Issues?.Count ?? 0) > 0)
                            {
                                if (result.Issues != null)
                                {
                                    foreach (var issue in result.Issues.Take(3))
                                    {
                                        logger.LogInformation(
                                            "  - {Id} [{Severity}/{Category}] scenes={Scenes} :: {Summary}", issue.Id,
                                            issue.Severity, issue.Category, string.Join(", ", issue.SceneIds ?? new()),
                                            issue.Summary);
                                    }
                                }
                            }
                            Interlocked.Increment(ref passed);
                        }
                        else
                        {
                            logger.LogError("[{Index}] {Name}: FAIL. ExpectedOk={ExpectedOk}, Assessment={Assessment}, Issues={IssueCount}", caseIndex, name, expectOk, result.OverallAssessment, result.Issues?.Count ?? 0);
                            if ((result.Issues?.Count ?? 0) > 0)
                            {
                                if (result.Issues != null)
                                {
                                    foreach (var issue in result.Issues.Take(3))
                                    {
                                        logger.LogError("  - {Id} [{Severity}/{Category}] scenes={Scenes} :: {Summary}",
                                            issue.Id, issue.Severity, issue.Category,
                                            string.Join(", ", issue.SceneIds ?? new()), issue.Summary);
                                    }
                                }
                            }
                            Interlocked.Increment(ref failed);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[{Index}] {Name}: Exception during evaluation", caseIndex, name);
                    Interlocked.Increment(ref failed);
                }
                finally
                {
                    gate.Release();
                }
            }));
        }

        await Task.WhenAll(tasks);

        logger.LogInformation("Consistency tests summary: {Passed} passed, {Failed} failed, {Total} total (excluding skipped)", passed, failed, passed + failed);
        return failed == 0 ? 0 : 1;
    }
}
