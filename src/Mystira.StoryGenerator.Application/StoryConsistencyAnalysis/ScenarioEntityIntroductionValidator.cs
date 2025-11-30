using Mystira.StoryGenerator.Application.Scenarios;
using Mystira.StoryGenerator.Contracts.Entities;
using Mystira.StoryGenerator.Domain.Stories;
using Mystira.StoryGenerator.GraphTheory.DataFlowAnalysis;

namespace Mystira.StoryGenerator.Application.StoryConsistencyAnalysis
{
    /// <summary>
    /// Validates that all entities used in each scene have been introduced
    /// on every possible path leading to that scene.
    ///
    /// This uses the generic graph-theoretic data-flow engine:
    /// <see cref="DataFlowAnalysis.ComputeMustIntroducedSets{TEntity}"/>
    /// and applies it to the Mystira <see cref="ScenarioGraph"/> plus
    /// <see cref="SceneEntity"/> metadata.
    /// </summary>
    public static class ScenarioEntityIntroductionValidator
    {
        /// <summary>
        /// Represents a "used before introduced" violation:
        /// an entity that is used in a scene but is not present
        /// in the "must-have-been introduced" set for that scene.
        /// </summary>
        public sealed record SceneReferenceViolation(
            string SceneId,
            SceneEntity Entity);

        /// <summary>
        /// An equality comparer for <see cref="SceneEntity"/> that
        /// treats entities as equal if their <see cref="SceneEntity.Type"/>
        /// and <see cref="SceneEntity.Name"/> match (case-insensitive).
        /// This is used by the data-flow analysis to track presence/absence.
        /// </summary>
        private sealed class SceneEntityComparer : IEqualityComparer<SceneEntity>
        {
            public static readonly SceneEntityComparer Instance = new();

            public bool Equals(SceneEntity? x, SceneEntity? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null || y is null) return false;

                return x.Type == y.Type &&
                       string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(SceneEntity obj)
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + obj.Type.GetHashCode();
                    hash = hash * 31 + StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name);
                    return hash;
                }
            }
        }

        /// <summary>
        /// Builds a data-flow view of the scenario and finds all entities
        /// that are used in a scene without having been introduced along
        /// every path to that scene.
        ///
        /// You provide three extraction functions:
        /// <list type="bullet">
        ///   <item><paramref name="getIntroduced"/> — which entities are introduced in this scene?</item>
        ///   <item><paramref name="getRemoved"/> — which entities are explicitly removed / forgotten?</item>
        ///   <item><paramref name="getUsed"/> — which entities are used / referenced in this scene?</item>
        /// </list>
        ///
        /// The method then:
        /// <list type="number">
        ///   <item>Constructs <see cref="DataFlowNode{TEntity}"/> instances for each <see cref="Scene"/>.</item>
        ///   <item>Runs <see cref="DataFlowAnalysis.ComputeMustIntroducedSets{TEntity}"/> with <see cref="SceneEntity"/> as <c>TEntity</c>.</item>
        ///   <item>For each scene, flags entities in <paramref name="getUsed"/> that are not in the corresponding must-set.</item>
        /// </list>
        /// </summary>
        /// <param name="graph">The scenario graph built from a <see cref="Scenario"/>.</param>
        /// <param name="getIntroduced">
        /// Function mapping a scene to the entities that are introduced by that scene.
        /// </param>
        /// <param name="getRemoved">
        /// Function mapping a scene to entities that are explicitly removed / forgotten in that scene.
        /// </param>
        /// <param name="getUsed">
        /// Function mapping a scene to entities that are used / referenced in that scene (for validation).
        /// </param>
        /// <returns>
        /// A list of <see cref="SceneReferenceViolation"/> instances, one for each
        /// "used-before-introduced on some path" problem detected.
        /// </returns>
        public static IReadOnlyList<SceneReferenceViolation> FindIntroductionViolations(
            ScenarioGraph graph,
            Func<Scene, IEnumerable<SceneEntity>> getIntroduced,
            Func<Scene, IEnumerable<SceneEntity>> getRemoved,
            Func<Scene, IEnumerable<SceneEntity>> getUsed)
        {
            ArgumentNullException.ThrowIfNull(graph);
            ArgumentNullException.ThrowIfNull(getIntroduced);
            ArgumentNullException.ThrowIfNull(getRemoved);
            ArgumentNullException.ThrowIfNull(getUsed);

            // Extract start scene id
            var startSceneId = graph.Roots().Count() switch
            {
                0 => throw new ArgumentException("Scenario must have at least one root scene"),
                1 => graph.Roots().First().Id,
                > 1 => throw new ArgumentException(
                    "Scenario must have exactly one root scene, more than one was found"),
                _ => throw new ArgumentOutOfRangeException()
            };

            // 1) Build DataFlowNode<SceneEntity> dictionary from ScenarioGraph
            var nodeMap = BuildDataFlowNodes(graph, getIntroduced, getRemoved);

            // 2) Run generic must-analysis with explicit type argument
            var mustSets = DataFlowAnalysis.ComputeMustIntroducedSets(
                nodeMap,
                startSceneId);

            // 3) Check each scene: used entity must be in must[scene]
            var violations = new List<SceneReferenceViolation>();
            foreach (var scene in graph.Nodes)
            {
                if (!mustSets.TryGetValue(scene.Id, out var mustForScene))
                    continue; // Should not happen, but be defensive

                var usedEntities = getUsed(scene);

                foreach (var used in usedEntities)
                    // Contains() respects our SceneEntityComparer via the HashSet setup.
                    if (!mustForScene.Contains(used)) violations.Add(new SceneReferenceViolation(scene.Id, used));
            }

            return violations;
        }

        /// <summary>
        /// Internal helper: creates a <see cref="DataFlowNode{TEntity}"/> for each scene
        /// in the <see cref="ScenarioGraph"/>, wiring predecessor/successor ids and
        /// attaching introduced/removed <see cref="SceneEntity"/> sets.
        /// </summary>
        private static IReadOnlyDictionary<string, DataFlowNode<SceneEntity>> BuildDataFlowNodes(
            ScenarioGraph graph,
            Func<Scene, IEnumerable<SceneEntity>> getIntroduced,
            Func<Scene, IEnumerable<SceneEntity>> getRemoved)
        {
            var dict = new Dictionary<string, DataFlowNode<SceneEntity>>();

            foreach (var scene in graph.Nodes)
            {
                var predecessors = graph.GetPredecessors(scene).Select(s => s.Id);
                var successors   = graph.GetSuccessors(scene).Select(s => s.Id);

                var introduced = new HashSet<SceneEntity>(
                    getIntroduced(scene),
                    SceneEntityComparer.Instance);

                var removed = new HashSet<SceneEntity>(
                    getRemoved(scene),
                    SceneEntityComparer.Instance);

                dict[scene.Id] = new DataFlowNode<SceneEntity>(
                    scene.Id,
                    predecessors,
                    successors,
                    introduced,
                    removed);
            }

            return dict;
        }
    }
}
