using ArchUnitNet.Common.Extraction;

namespace ArchUnitNet.Common.Projection.Cycles;

/// <summary>
/// Johnson's algorithm for finding all elementary cycles in a directed graph.
/// Time complexity: O((V + E) * (1 + number of cycles))
/// An elementary cycle has no repeated vertices except the start/end node.
/// </summary>
public class JohnsonsCycles
{
    private readonly Graph _graph;
    private readonly TarjanSCC _tarjan;
    private List<List<string>> _cycles = new();
    private HashSet<string> _blocked = new();
    private Dictionary<string, HashSet<string>> _blockedMap = new();
    private Stack<string> _path = new();

    public JohnsonsCycles(Graph graph)
    {
        _graph = graph;
        _tarjan = new TarjanSCC(graph);
    }

    /// <summary>
    /// Find all elementary cycles in the graph.
    /// Returns list of cycles, where each cycle is a list of nodes forming the cycle.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<string>> FindAllCycles()
    {
        _cycles.Clear();

        var allNodes = ProjectNodes.ExtractAllNodes(_graph);
        var sortedNodes = allNodes.OrderBy(n => n).ToList();

        // Process each node as potential cycle start
        for (int i = 0; i < sortedNodes.Count; i++)
        {
            var startNode = sortedNodes[i];
            _blocked.Clear();
            _blockedMap.Clear();
            _path.Clear();

            // Only consider SCCs that contain this node and nodes after it (to avoid duplicates)
            var stronglyConnectedNodes = GetStronglyConnectedNodes(startNode);
            foreach (var n in stronglyConnectedNodes)
            {
                if (sortedNodes.IndexOf(n) >= i)
                {
                    _blocked.Add(n);
                    _blockedMap[n] = new HashSet<string>();
                }
            }

            _path.Push(startNode);
            FindCyclesFromNode(startNode, startNode);
            _blocked.Remove(startNode);
        }

        return _cycles.AsReadOnly();
    }

    /// <summary>
    /// Find cycles starting and ending at a given node.
    /// Uses depth-first search with backtracking.
    /// </summary>
    private bool FindCyclesFromNode(string currentNode, string startNode)
    {
        bool foundCycle = false;
        var dependencies = ProjectNodes.ExtractDependencies(_graph, currentNode);

        foreach (var nextNode in dependencies)
        {
            if (nextNode == startNode && _path.Count > 1)
            {
                // Found a cycle back to start
                var cycle = new List<string>(_path);
                cycle.Reverse();
                _cycles.Add(cycle);
                foundCycle = true;
            }
            else if (!_blocked.Contains(nextNode))
            {
                _path.Push(nextNode);
                if (FindCyclesFromNode(nextNode, startNode))
                {
                    foundCycle = true;
                }
                _path.Pop();
            }
        }

        if (foundCycle)
        {
            Unblock(currentNode);
        }
        else
        {
            foreach (var nextNode in dependencies)
            {
                if (!_blockedMap.ContainsKey(nextNode))
                {
                    _blockedMap[nextNode] = new HashSet<string>();
                }
                _blockedMap[nextNode].Add(currentNode);
            }
        }

        return foundCycle;
    }

    /// <summary>
    /// Unblock a node and recursively unblock nodes that depend on it.
    /// This is part of Johnson's algorithm to avoid false positives.
    /// </summary>
    private void Unblock(string node)
    {
        if (_blocked.Contains(node))
        {
            _blocked.Remove(node);

            if (_blockedMap.ContainsKey(node))
            {
                var toUnblock = new HashSet<string>(_blockedMap[node]);
                foreach (var dependent in toUnblock)
                {
                    Unblock(dependent);
                }
                _blockedMap[node].Clear();
            }
        }
    }

    /// <summary>
    /// Get all nodes in the same strongly connected component as the given node.
    /// Returns nodes that can reach each other through dependencies.
    /// </summary>
    private HashSet<string> GetStronglyConnectedNodes(string node)
    {
        var sccs = _tarjan.FindSCCs();
        var result = new HashSet<string>();

        foreach (var scc in sccs)
        {
            if (scc.Contains(node))
            {
                foreach (var n in scc)
                {
                    result.Add(n);
                }
                break;
            }
        }

        return result;
    }
}
