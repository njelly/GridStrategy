////////////////////////////////////////////////////////////////////////////////
//
//  Board (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 12/11/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using TofuCore;
using Tofunaut.SharpUnity;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class Board : SharpGameObject
    {
        public BoardTile this[int x, int y]
        {
            get
            {
                return _tiles[x, y];
            }
        }

        public Vector3 CenterPos
        {
            get
            {
                return new Vector3(BoardTileView.Size * width / 2f, LocalPosition.y, BoardTileView.Size * height / 2f) 
                    - new Vector3(BoardTileView.Size / 2f, 0, BoardTileView.Size / 2f);
            }
        }

        public BoardTile HighlightedTile { get; private set; }

        public readonly int width;
        public readonly int height;

        private readonly BoardTile[,] _tiles;
        private readonly Game _game;
        private readonly Plane _groundPlane;

        // --------------------------------------------------------------------------------------------
        public Board(Game game, int width, int height) : base("Board")
        {
            _game = game;
            this.width = width;
            this.height = height;

            _tiles = new BoardTile[width, height];
            for(int x = 0; x < width; x++)
            {
                for(int y = 0; y < height; y++)
                {
                    BoardTile boardTile = new BoardTile(x, y);
                    AddChild(boardTile);
                    _tiles[x, y] = boardTile;
                }
            }

            _groundPlane = new Plane(Vector3.up, Vector3.zero);
        }

        // --------------------------------------------------------------------------------------------
        public BoardTile GetHeroStartTile(int playerIndex)
        {
            switch(playerIndex)
            {
                case 0:
                    return this[0, 0];
                case 1:
                    return this[width - 1, height - 1];
                default:
                    Debug.LogError($"Unhandled player index for hero start tile: {playerIndex}");
                    return null;
            }
        }

        // --------------------------------------------------------------------------------------------
        public BoardTile GetTile(int x, int y) => GetTile(new IntVector2(x, y));
        public BoardTile GetTile(IntVector2 coord)
        {
            if(coord.x < 0 || coord.x >= width)
            {
                return null;
            }
            if(coord.y < 0 || coord.y >= height)
            {
                return null;
            }

            return this[coord.x, coord.y];
        }

        // --------------------------------------------------------------------------------------------
        protected override void Build() { }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the move cost of a path for a unit.
        /// </summary>
        public int CalculatePathCost(IntVector2[] path, Unit unit)
        {
            List<BoardTile> allBoardTilesInPath = GetAllBoardTilesInPath(path);
            int cost = 0;
            foreach(BoardTile boardTile in allBoardTilesInPath)
            {
                cost += boardTile.GetMoveCostForUnit(unit);
            }
            return cost;
        }

        // --------------------------------------------------------------------------------------------
        public List<BoardTile> GetAllBoardTilesInPath(IntVector2[] path)
        {
            List<BoardTile> toReturn = new List<BoardTile>();
            if (path == null)
            {
                return toReturn;
            }
            if(!IsPathValid(path))
            {
                Debug.LogError("path is not valid");
                return toReturn;
            }


            for(int i = 0; i < path.Length; i++)
            {
                toReturn.Add(GetTile(path[i]));
                if(i >= path.Length - 1)
                {
                    break;
                }

                IntVector2 step = path[i].StepToward(path[i + 1]);
                while(!step.Equals(path[i + 1]))
                {
                    toReturn.Add(GetTile(step));
                    step = step.StepToward(path[i + 1]);
                }
            }

            return toReturn;
        }

        // --------------------------------------------------------------------------------------------
        public void HighlightBoardTilesForUnitMove(Unit unit)
        {
            ClearAllBoardTileHighlights();
            HighlightBoardTilesForUnitMoveRecursive(unit, new Dictionary<BoardTile, int>(), unit.BoardTile, 0);
        }

        // --------------------------------------------------------------------------------------------
        private void HighlightBoardTilesForUnitMoveRecursive(Unit unit, Dictionary<BoardTile, int> visitedToCost, BoardTile current, int cost)
        {
            if(current == null)
            {
                return;
            }

            if (cost > unit.MoveRange)
            {
                return;
            }

            if (visitedToCost.ContainsKey(current))
            {
                int previousCost = visitedToCost[current];
                if (cost < previousCost)
                {
                    visitedToCost[current] = cost;
                }
                else
                {
                    // we've alredy visited this tile and it was cheaper then, so we're done
                    return;
                }
            }
            else
            {
                visitedToCost.Add(current, cost);
            }

            if(BoardTileView.TryGetView(current, out BoardTileView view))
            {
                view.SetHighlight(BoardTileView.EHighlight.Move);
            }

            BoardTile northTile = GetTile(current.xCoord, current.yCoord + 1);
            if(northTile != null)
            {
                HighlightBoardTilesForUnitMoveRecursive(unit, visitedToCost, northTile, cost + northTile.GetMoveCostForUnit(unit));
            }

            BoardTile southTile = GetTile(current.xCoord, current.yCoord - 1);
            if (southTile != null)
            {
                HighlightBoardTilesForUnitMoveRecursive(unit, visitedToCost, southTile, cost + southTile.GetMoveCostForUnit(unit));
            }

            BoardTile eastTile = GetTile(current.xCoord + 1, current.yCoord);
            if (eastTile != null)
            {
                HighlightBoardTilesForUnitMoveRecursive(unit, visitedToCost, eastTile, cost + eastTile.GetMoveCostForUnit(unit));
            }

            BoardTile westTile = GetTile(current.xCoord - 1, current.yCoord);
            if (westTile != null)
            {
                HighlightBoardTilesForUnitMoveRecursive(unit, visitedToCost, westTile, cost + westTile.GetMoveCostForUnit(unit));
            }
        }

        // --------------------------------------------------------------------------------------------
        public void HighlightBoardTilesForUseSkill(Skill skill)
        {
            ClearAllBoardTileHighlights();

            List<BoardTile> targetableTiles = skill.GetTargetableTiles();
            foreach(BoardTile boardTile in targetableTiles)
            {
                if (BoardTileView.TryGetView(boardTile, out BoardTileView view))
                {
                    view.SetHighlight(skill.DamageDealt > 0 ? BoardTileView.EHighlight.Attack : BoardTileView.EHighlight.Heal);
                }
            }
        }

        // --------------------------------------------------------------------------------------------
        public void HighlightBoardTile(IntVector2 coord)
        {
            ClearAllBoardTileHighlights();

            if(BoardTileView.TryGetView(this.GetTile(coord), out BoardTileView view))
            {
                view.SetHighlight(BoardTileView.EHighlight.Neutral);
                HighlightedTile = view.BoardTile;
            }
        }

        // --------------------------------------------------------------------------------------------
        public void ClearAllBoardTileHighlights()
        {
            foreach(BoardTile boardTile in _tiles)
            {
                if(BoardTileView.TryGetView(boardTile, out BoardTileView boardTileView))
                {
                    boardTileView.SetHighlight(BoardTileView.EHighlight.None);
                }
            }

            HighlightedTile = null;
        }

        // --------------------------------------------------------------------------------------------
        public BoardTile GetBoardTileAtPosition(Vector3 worldPosition)
        {
            IntVector2 coord = new IntVector2(Mathf.RoundToInt(worldPosition.x / BoardTileView.Size), Mathf.RoundToInt(worldPosition.z / BoardTileView.Size));
            return GetTile(coord);
        }

        // --------------------------------------------------------------------------------------------
        public bool RaycastToPlane(Vector2 mousePos, out Vector3 worldPos) => RaycastToPlane(_game.gameCamera.ScreenPointToRay(mousePos), out worldPos);
        public bool RaycastToPlane(Ray ray, out Vector3 worldPos)
        {
            if (_groundPlane.Raycast(ray, out float distance))
            {
                worldPos = ray.GetPoint(distance);
                return true;
            }

            worldPos = Vector3.zero;
            return false;
        }


        #region static functions

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns true if the coordinate is contained in one of the linesegments represented by the path.
        /// </summary>
        public static bool DoesPathContainCoord(IntVector2[] path, IntVector2 coord)
        {
            if (!IsPathValid(path))
            {
                Debug.LogError("aborted DoesPathContainCoord, path is not valid");
                return false;
            }

            if (path == null)
            {
                return false;
            }

            if (path.Length <= 0)
            {
                return false;
            }

            if (coord.Equals(path[0]))
            {
                return true;
            }

            bool toReturn = false;
            for (int i = 1; i < path.Length; i++)
            {
                toReturn |= coord.Equals(path[i]) || coord.IsCollinearAndBetween(path[i - 1], path[i]);
            }

            return toReturn;
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Prints a string representation of a path to the console.
        /// </summary>
        public static void DebugPrintPath(IntVector2[] path)
        {
            if (path == null)
            {
                Debug.Log("NULL PATH");
                return;
            }
            if (path.Length == 0)
            {
                Debug.Log("EMPTY PATH");
                return;
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < path.Length; i++)
            {
                sb.Append(path[i].ToString());
                sb.Append(" | ");
            }
            Debug.Log(sb.ToString());
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a path equivelent to the portion of a path up to and including the coord.
        /// </summary>
        public static IntVector2[] BacktrackTo(IntVector2[] path, IntVector2 coord)
        {
            if (!IsPathValid(path))
            {
                Debug.LogError("can't backtrack, path is not valid");
                return path;
            }

            // edge case when backtracking to the first square
            if (coord.Equals(path[0]))
            {
                return new[] { path[0] };
            }

            List<IntVector2> pathAsList = new List<IntVector2>();
            pathAsList.Add(path[0]);

            for (int i = 1; i < path.Length; i++)
            {
                if (coord.Equals(path[i]) || coord.IsCollinearAndBetween(path[i - 1], path[i]))
                {
                    pathAsList.Add(coord);
                    break;
                }

                pathAsList.Add(path[i]);
            }

            return pathAsList.ToArray();
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Recursively simplifies a path, so that each element represents a beginning, end, or change
        /// of direction on the path. (So there are never 3 collinear elements in a row.)
        /// </summary>
        public static IntVector2[] SimplifyPath(IntVector2[] path)
        {
            if (!IsPathValid(path))
            {
                Debug.LogError("can't simplify, path is not valid");
                return path;
            }

            bool removePoint = false;
            int i;
            for (i = 1; i < path.Length; i++)
            {
                if (path[i].Equals(path[i - 1]))
                {
                    // remove equal coords
                    removePoint = true;
                    break;
                }

                if (i < path.Length - 1 && path[i].IsCollinear(path[i - 1], path[i + 1]))
                {
                    // remove coords that are collinear with the previous and next coords
                    removePoint = true;
                    break;
                }
            }

            // keep attempting to simplify the path until there are no more points to be removed
            if (removePoint)
            {
                List<IntVector2> pathAsList = new List<IntVector2>(path);
                pathAsList.RemoveAt(i);
                path = pathAsList.ToArray();

                return SimplifyPath(path);
            }
            else
            {
                return path;
            }
        }

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns true if and only if each element of a path is collinear with the previous element.
        /// </summary>
        public static bool IsPathValid(IntVector2[] path)
        {
            for(int i = 1; i < path.Length; i++)
            {
                if(!path[i].IsCollinear(path[i - 1]))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion static functions
    }
}
