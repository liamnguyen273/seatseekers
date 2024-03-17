using System;
using System.Collections.Generic;
using System.Linq;

namespace com.tinycastle.SeatCinema
{
    public class DijkstraPathfindEngine<TKey>
    {
        private Func<TKey, TKey, int> _moveCostQuery;
        private IComparer<TKey> _distanceQuery;
        private Func<TKey, IEnumerable<TKey>> _neighborQuery;
        private TKey _start;
        
        private Dictionary<TKey, int> _cost = new();
        private Dictionary<TKey, TKey> _parent = new();
        private HashSet<TKey> _visited = new();
        private SortedSet<TKey> _frontier;
        
        public DijkstraPathfindEngine(TKey start, Func<TKey, TKey, int> moveCostQuery, IComparer<TKey> distanceQuery, Func<TKey, IEnumerable<TKey>> neighborQuery)
        {
            _moveCostQuery = moveCostQuery;
            _distanceQuery = distanceQuery;
            _neighborQuery = neighborQuery;
            _start = start;

            _frontier = new SortedSet<TKey>(_distanceQuery);
            
            Clear();
        }

        public void Run()
        {
            _cost[_start] = 0;
            _frontier.Add(_start);

            while (_frontier.Count > 0)
            {
                var node = _frontier!.Min;
                _frontier.Remove(node);

                foreach (var neighbor in _neighborQuery(node))
                {
                    var currNeighborCost = GetCost(neighbor);
                    var thisPathCost = GetCost(node) + _moveCostQuery(node, neighbor);
                    if (currNeighborCost > thisPathCost)
                    {
                        SetCost(neighbor, thisPathCost);
                        SetParent(neighbor, node);
                        _frontier.Add(neighbor);
                    }
                }
            }
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
            _frontier.Clear();
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
    }
}