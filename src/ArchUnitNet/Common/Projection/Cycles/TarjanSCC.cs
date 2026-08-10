using ArchUnitNet.Common.Extraction;

namespace ArchUnitNet.Common.Projection.Cycles;

/// <summary>
/// Tarjan's algorithm for finding strongly connected components (SCCs) in a directed graph.
/// Time complexity: O(V + E) where V = vertices, E = edges.
/// Used as foundation for cycle detection.
/// </summary>
public class TarjanSCC
{
    private readonly Graph _graph;
    private readonly Dictionary<string, int> _indices = new();
    private readonly Dictionary<string, int> _lowlinks = new();
    private readonly Stack<string> _stack = new();
    private readonly List<List<string>> _sccs = new();
    private int _indexCounter;

    public TarjanSCC(Graph graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// Find all strongly connected components in the graph.
    /// Returns list of SCCs, where each SCC is a list of nodes.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<string>> FindSCCs()
    {
        var allNodes = ProjectNodes.ExtractAllNodes(_graph);

        foreach (var node in allNodes)
        {
            if (!_indices.ContainsKey(node))
            {
                StrongConnect(node);
            }
        }

        return _sccs.AsReadOnly();
    }

    /// <summary>
    /// Get only the SCCs that contain cycles (size > 1 or self-loop).
    /// </summary>
    public IReadOnlyList<IReadOnlyList<string>> FindCyclicSCCs()
    {
        var allSCCs = FindSCCs();
        return allSCCs
            .Where(scc => scc.Count > 1 || HasSelfLoop(scc[0]))
            .ToList()
            .AsReadOnly();
    }

    private void StrongConnect(string node)
    {
        _indices[node] = _indexCounter;
        _lowlinks[node] = _indexCounter;
        _indexCounter++;
        _stack.Push(node);

        // Follow all outgoing edges
        var dependencies = ProjectNodes.ExtractDependencies(_graph, node);
        foreach (var dependent in dependencies)
        {
            if (!_indices.ContainsKey(dependent))
            {
                StrongConnect(dependent);
                _lowlinks[node] = Math.Min(_lowlinks[node], _lowlinks[dependent]);
            }
            else if (IsOnStack(dependent))
            {
                _lowlinks[node] = Math.Min(_lowlinks[node], _indices[dependent]);
            }
        }

        // If node is a root node, pop the stack and create SCC
        if (_lowlinks[node] == _indices[node])
        {
            var scc = new List<string>();
            string w;
            do
            {
                w = _stack.Pop();
                scc.Add(w);
            } while (w != node);

            if (scc.Count > 0)
            {
                _sccs.Add(scc);
            }
        }
    }

    private bool IsOnStack(string node)
    {
        return _stack.Contains(node);
    }

    private bool HasSelfLoop(string node)
    {
        return _graph.Edges.Any(e => e.Source == node && e.Target == node);
    }
}
