using Mystira.StoryGenerator.Domain.Graph;

namespace Mystira.StoryGenerator.GraphTheory.Algorithms;

public static class DominatorAlgorithms
{
    /// <summary>
    /// Computes the immediate dominators for all nodes reachable from a start node
    /// in a directed graph using the Lengauer-Tarjan algorithm.
    /// Works correctly on graphs with cycles.
    /// </summary>
    /// <returns>A dictionary mapping each node to its immediate dominator.</returns>
    public static Dictionary<TNode, TNode> GetImmediateDominators<TNode, TEdgeLabel>(
        this IDirectedGraph<TNode, TEdgeLabel> graph,
        TNode start)
        where TNode : notnull
    {
        var dfnum = new Dictionary<TNode, int>();
        var vertex = new List<TNode>();
        var parent = new Dictionary<TNode, TNode>();
        var semi = new Dictionary<TNode, int>();
        var idom = new Dictionary<TNode, TNode>();
        var ancestor = new Dictionary<TNode, TNode?>();
        var best = new Dictionary<TNode, TNode>();
        var bucket = new Dictionary<TNode, List<TNode>>();

        // Step 1: DFS to compute semi-dominators
        int n = 0;
        DFS(start, default!);

        void DFS(TNode v, TNode p)
        {
            dfnum[v] = ++n;
            vertex.Add(v);
            parent[v] = p;
            semi[v] = dfnum[v];
            ancestor[v] = default;
            best[v] = v;
            bucket[v] = new List<TNode>();

            foreach (var w in graph.GetSuccessors(v))
            {
                if (!dfnum.ContainsKey(w))
                {
                    DFS(w, v);
                }
            }
        }

        TNode Eval(TNode v)
        {
            if (EqualityComparer<TNode>.Default.Equals(ancestor[v], default))
                return v;
            Compress(v);
            return best[v];
        }

        void Compress(TNode v)
        {
            var a = ancestor[v];
            if (!EqualityComparer<TNode>.Default.Equals(a, default) && a != null)
            {
                if (ancestor.TryGetValue(a, out var ancestorOfA) && ancestorOfA != null && !EqualityComparer<TNode>.Default.Equals(ancestorOfA, default))
                {
                    Compress(a);
                    if (semi[best[a!]] < semi[best[v]])
                    {
                        best[v] = best[a!];
                    }
                    ancestor[v] = ancestorOfA!;
                }
            }
        }

        void Link(TNode v, TNode w)
        {
            ancestor[w] = v;
        }

        // Step 2 & 3: Compute semi-dominators and initial immediate dominators
        for (int i = n - 1; i > 0; i--)
        {
            var w = vertex[i];
            foreach (var v in graph.GetPredecessors(w))
            {
                if (!dfnum.ContainsKey(v))
                    continue;
                var u = Eval(v);
                if (semi[u] < semi[w])
                {
                    semi[w] = semi[u];
                }
            }
            var semiVertex = vertex[semi[w] - 1];
            bucket[semiVertex].Add(w);
            Link(parent[w], w);

            foreach (var v in bucket[parent[w]])
            {
                var u = Eval(v);
                idom[v] = semi[u] < semi[v] ? u : parent[w];
            }
            bucket[parent[w]].Clear();
        }

        // Step 4: Finalize immediate dominators
        for (int i = 1; i < n; i++)
        {
            var w = vertex[i];
            if (!idom[w].Equals(vertex[semi[w] - 1]))
            {
                idom[w] = idom[idom[w]];
            }
        }

        idom[start] = start; // Start node dominates itself
        return idom;
    }
}
