using System;
using System.Collections.Generic;
using System.Linq;
using com.brg.Common;

namespace com.tinycastle.SeatSeekers
{
    public class DijkstraPathfindEngine<TKey>
    {
        private Func<TKey, TKey, int> _moveCostQuery;
        private Func<TKey, int> _priorityQuery;
        private Func<TKey, IEnumerable<TKey>> _neighborQuery;
        private TKey _start;
        
        private Dictionary<TKey, int> _cost = new();
        private Dictionary<TKey, TKey> _parent = new();
        private HashSet<TKey> _visited = new();
        private PriorityQueue<TKey> _frontier;
        
        public DijkstraPathfindEngine(TKey start, Func<TKey, TKey, int> moveCostQuery, Func<TKey, int> priorityQuery, Func<TKey, IEnumerable<TKey>> neighborQuery)
        {
            _moveCostQuery = moveCostQuery;
            _priorityQuery = priorityQuery;
            _neighborQuery = neighborQuery;
            _start = start;

            _frontier = new PriorityQueue<TKey>();
            
            Clear();
        }

        public void Run()
        {
            _cost[_start] = 0;
            _frontier.Enqueue(_start, _priorityQuery(_start));

            var iterations = 0;
            
            while (_frontier.Count > 0)
            {
                ++iterations;
                var node = _frontier.Dequeue();

                foreach (var neighbor in _neighborQuery(node))
                {
                    var currNeighborCost = GetCost(neighbor);
                    var thisPathCost = InfSafeSum(GetCost(node), _moveCostQuery(node, neighbor));
                    
                    if (currNeighborCost > thisPathCost)
                    {
                        SetCost(neighbor, thisPathCost);
                        SetParent(neighbor, node);
                        _frontier.Enqueue(neighbor, _priorityQuery(neighbor));
                    }
                }
            }
            
            LogObj.Default.Info("Dijkstra", $"Dijkstra run on Car in {iterations} iterations");
        }

        public bool GetPathTo(TKey destination, out List<TKey>? path)
        {
            if (GetCost(destination) == int.MaxValue)
            {
                path = null;
                return false;
            }
            
            var result = new List<TKey>();
            result.Add(destination);

            var currNode = destination;
            while (GetParent(currNode, out var parent))
            {
                result.Add(parent);
                currNode = parent;
            }

            if (!_start!.Equals(result.Last()))
            {
                path = null;
                return false;
            }
            
            path = result;
            return true;
        }

        public void Clear()
        {
            _cost.Clear();
            _parent.Clear();
            _visited.Clear();
            _frontier = new PriorityQueue<TKey>();
        }

        private bool IsVisited(TKey key)
        {
            return _visited.Contains(key);
        }

        private int GetCost(TKey key)
        {
            if (_cost.TryGetValue(key, out var cost)) return cost;
            return int.MaxValue;
        }

        private void SetCost(TKey key, int cost)
        {
            if (cost == int.MaxValue) return;
            _cost[key] = cost;
        }

        private bool GetParent(TKey node, out TKey parent)
        {
            return _parent.TryGetValue(node, out parent);
        }

        private void SetParent(TKey node, TKey parent)
        {
            _parent[node] = parent;
        }

        private static int InfSafeSum(int a, int b)
        {
            if (a == int.MaxValue || b == int.MaxValue) return int.MaxValue;
            return a + b;
        }
    }
}