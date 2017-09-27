﻿namespace Algorithms
{
    using Grid;
    using System.Collections.Generic;
    using System.Linq;

    public class AStar : AlgorithmBase
    {
        private readonly List<Node> _openList = new List<Node>();
        private readonly List<Node> _closedList = new List<Node>();
        private readonly Coord _destination;
        private int _id;
        private Node _currentNode;
        private readonly List<Coord> _neighbours;

        public AStar(Grid grid) : base(grid)
        {
            AlgorithmName = "A*";
            var origin = Grid.GetStart().Coord;
            _destination = Grid.GetEnd().Coord;
            _id = 1;
            _neighbours = new List<Coord>();

            // Put the origin on the open list
            _openList.Add(new Node(_id++, null, origin.X, origin.Y, 0, GetH(origin, _destination)));
        }

        public override SearchDetails GetPathTick()
        {
            if (_currentNode == null)
            {
                if (!_openList.Any()) return GetSearchDetails();
                
                // Take the current node off the open list to be examined
                _currentNode = _openList.OrderBy(x => x.F).First();

                // Move it to the closed list so it doesn't get examined again
                _openList.Remove(_currentNode);
                _closedList.Add(_currentNode);
                Grid.SetCell(_currentNode.Coord.X, _currentNode.Coord.Y, Enums.CellType.Closed);

                _neighbours.AddRange(GetNeighbours(_currentNode));
            }

            if (_neighbours.Any())
            {
                Grid.SetCell(_currentNode.Coord.X, _currentNode.Coord.Y, Enums.CellType.Current);

                var thisNeighbour = _neighbours.First();
                _neighbours.Remove(thisNeighbour);

                // If the neighbour is the destination
                if (CoordsMatch(thisNeighbour, _destination))
                {
                    // Construct the path by tracing back through the closed list until there are no more parent id references
                    Path = new List<Coord> { thisNeighbour };
                    int? parentId = _currentNode.Id;
                    while (parentId.HasValue)
                    {
                        var nextNode = _closedList.First(x => x.Id == parentId);
                        Path.Add(nextNode.Coord);
                        parentId = nextNode.ParentId;
                    }

                    // Reorder the path to be from origin to destination and return
                    Path.Reverse();

                    return GetSearchDetails();
                }

                // Get the cost of the current node plus the extra step and heuristic
                var hFromHere = GetH(thisNeighbour, _destination);
                var neighbourCost = _currentNode.G + 1 + hFromHere;

                // Check if the node is on the open list already and if it has a higher cost path
                var openListItem = _openList.FirstOrDefault(x => x.Id == GetExistingNode(true, thisNeighbour));
                if (openListItem != null && openListItem.F > neighbourCost)
                {
                    // Repoint the openlist node to use this lower cost path
                    openListItem.F = neighbourCost;
                    openListItem.ParentId = _currentNode.Id;
                }

                // Check if the node is on the closed list already and if it has a higher cost path
                var closedListItem = _closedList.FirstOrDefault(x => x.Id == GetExistingNode(false, thisNeighbour));
                if (closedListItem != null && closedListItem.F > neighbourCost)
                {
                    // Repoint the closedlist node to use this lower cost path
                    closedListItem.F = neighbourCost;
                    closedListItem.ParentId = _currentNode.Id;
                }

                // If the neighbour node isn't on the open or closed list, add it
                if (openListItem != null || closedListItem != null) return GetSearchDetails();
                _openList.Add(new Node(_id++, _currentNode.Id, thisNeighbour.X, thisNeighbour.Y, _currentNode.G + 1, hFromHere));
                Grid.SetCell(thisNeighbour.X, thisNeighbour.Y, Enums.CellType.Open);
            }
            else
            {
                Grid.SetCell(_currentNode.Coord.X, _currentNode.Coord.Y, Enums.CellType.Closed);
                _currentNode = null;
                return GetPathTick();
            }

            return GetSearchDetails();
        }

        private static int GetH(Coord origin, Coord destination)
        {
            return GetManhattenDistance(origin, destination);
        }

        private int? GetExistingNode(bool checkOpenList, Coord coordToCheck)
        {
            return checkOpenList ? _openList.FirstOrDefault(x => CoordsMatch(x.Coord, coordToCheck))?.Id : _closedList.FirstOrDefault(x => CoordsMatch(x.Coord, coordToCheck))?.Id;
        }

        protected override SearchDetails GetSearchDetails()
        {
            return new SearchDetails
            {
                Path = Path?.ToArray(),
                LastNode = _currentNode,
                DistanceOfCurrentNode = _currentNode == null ? 0 : GetH(_currentNode.Coord, _destination),
                OpenListSize = _openList.Count,
                ClosedListSize = _closedList.Count,
                UnexploredListSize = Grid.GetCountOfType(Enums.CellType.Empty),
                Operations = Operations++
            };
        }
    }
}
