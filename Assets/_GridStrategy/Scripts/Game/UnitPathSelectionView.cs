////////////////////////////////////////////////////////////////////////////////
//
//  UnitPathSelectionView (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 01/26/2020
//
////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using TofuCore;
using Tofunaut.SharpUnity;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class UnitPathSelectionView : SharpLineRenderer
    {
        private const float PathHeight = 1.5f;

        public IntVector2[] CurrentPath { get; private set; }

        public Unit unit;

        private readonly Game _game;

        // --------------------------------------------------------------------------------------------
        public UnitPathSelectionView(Game game) : base("UnitPathSelectionView", new[] { AppManager.AssetManager.Get<Material>(AssetPaths.Materials.WaypointPath) }, new Vector3[0])
        {
            _game = game;

            TexOffsetAnimVelocity = new Vector2(-1f, 0f);
            TextureMode = LineTextureMode.Tile;
            UseWorldSpace = true;
        }

        // --------------------------------------------------------------------------------------------
        protected override void PostRender()
        {
            base.PostRender();

            _game.board.HighlightBoardTilesForUnitMove(unit);

            SetPostionsBasedOnCurrentPath();
        }

        // --------------------------------------------------------------------------------------------
        public override void Destroy()
        {
            base.Destroy();

            _game.board.ClearAllBoardTileHighlights();

            CurrentPath = null;
        }

        // --------------------------------------------------------------------------------------------
        public void PathTo(BoardTile boardTile)
        {
            // return when no unit has been set
            if(unit == null)
            {
                return;
            }

            // return it is not the selected unit's owner's turn
            if(unit.Owner.playerIndex != _game.CurrentPlayer.playerIndex)
            {
                CurrentPath = null;
                return;
            }

            if(CurrentPath == null || CurrentPath.Length == 0)
            {
                CurrentPath = new[] { unit.BoardTile.Coord };
            }

            int alreadyContainsTileAtIndex = -1;
            for(int i = 0; i < CurrentPath.Length; i++)
            {
                if(CurrentPath[i].Equals(boardTile.Coord))
                {
                    alreadyContainsTileAtIndex = i;
                    break;
                }
            }
            if(alreadyContainsTileAtIndex >= 0)
            {
                // if the path already contains this tile, backtrack to it and set the currentPath to that
                IntVector2[] backtrackedPath = new IntVector2[alreadyContainsTileAtIndex + 1];
                for(int i = 0; i <= alreadyContainsTileAtIndex; i++)
                {
                    backtrackedPath[i] = CurrentPath[i];
                }
                CurrentPath = Board.RemoveDuplicates(backtrackedPath);
            }
            else
            {
                // the path does not contain this tile, so lets try to find the best path to it
                for (int backtrackIndex = CurrentPath.Length - 1; backtrackIndex >= 0; backtrackIndex--)
                {
                    // backtrack along our CurrentPath until we find a coord that has a valid path to the target
                    IntVector2[] upToBacktrackIndex = new List<IntVector2>(CurrentPath).GetRange(0, backtrackIndex + 1).ToArray();
                    int cost = _game.board.CalculateCostForPath(unit, upToBacktrackIndex);
                    if (_game.board.TryGetBestPathForUnit(unit, CurrentPath[backtrackIndex], boardTile.Coord, cost, out IntVector2[] backtrackAppend))
                    {
                        // we found a valid path, lets go with that
                        List<IntVector2> appendedPathAsList = new List<IntVector2>(upToBacktrackIndex);
                        for (int i = upToBacktrackIndex.Length; i < upToBacktrackIndex.Length + backtrackAppend.Length; i++)
                        {
                            appendedPathAsList.Add(backtrackAppend[i - upToBacktrackIndex.Length]);
                        }
                        CurrentPath = Board.RemoveDuplicates(appendedPathAsList.ToArray());
                        break;
                    }
                }
            }

            SetPostionsBasedOnCurrentPath();
        }

        // --------------------------------------------------------------------------------------------
        private void SetPostionsBasedOnCurrentPath()
        {
            if(CurrentPath == null || CurrentPath.Length == 0)
            {
                Positions = new Vector3[0];
                return;
            }

            List<Vector3> positionsAsList = new List<Vector3>();
            IntVector2[] simplifiedPath = Board.SimplifyPath(CurrentPath);
            foreach(IntVector2 pathPoint in simplifiedPath)
            {
                positionsAsList.Add(new Vector3(pathPoint.x * BoardTileView.Size, PathHeight, pathPoint.y * BoardTileView.Size));
            }

            Positions = positionsAsList.ToArray();
        }
    }
}