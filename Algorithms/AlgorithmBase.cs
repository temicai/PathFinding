﻿namespace Algorithms
{
    using Grid;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class AlgorithmBase
    {
        protected readonly Grid Grid;
        protected List<Coord> Path;
        protected int Operations;
        public string AlgorithmName;

        protected AlgorithmBase(Grid grid)
        {
            Grid = grid;
            Operations = 0;
        }

        public abstract SearchDetails GetPathTick();

        protected virtual IEnumerable<Coord> GetNeighbours(Node current)
        {
            var neighbours = new List<Cell>
            {
                Grid.GetCell(current.Coord.X - 1, current.Coord.Y),
                Grid.GetCell(current.Coord.X + 1, current.Coord.Y),
                Grid.GetCell(current.Coord.X, current.Coord.Y - 1),
                Grid.GetCell(current.Coord.X, current.Coord.Y + 1)
            };

            return neighbours.Where(x => x.Type != Enums.CellType.Invalid && x.Type != Enums.CellType.Solid).Select(x => x.Coord).ToArray();
        }

        protected abstract SearchDetails GetSearchDetails();

        protected static bool CoordsMatch(Coord a, Coord b) => a.X == b.X && a.Y == b.Y;

        protected static int GetManhattenDistance(Coord origin, Coord destination)
        {
            return Math.Abs(origin.X - destination.X) + Math.Abs(origin.Y - destination.Y);
        }
}
}
