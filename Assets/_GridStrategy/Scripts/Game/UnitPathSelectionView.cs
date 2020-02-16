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
                // otherwise, find the best path from the last element of current path to the tile
                IntVector2[] toAppend = _game.board.BestPathForUnit(unit, CurrentPath[CurrentPath.Length - 1], boardTile.Coord, CalculateCostForCurrentPath());
                List<IntVector2> appendedPathAsList = new List<IntVector2>(CurrentPath);
                for(int i = CurrentPath.Length; i < CurrentPath.Length + toAppend.Length; i++)
                {
                    appendedPathAsList.Add(toAppend[i - CurrentPath.Length]);
                }
                CurrentPath = Board.RemoveDuplicates(appendedPathAsList.ToArray());
            }

            SetPostionsBasedOnCurrentPath();
        }

        // --------------------------------------------------------------------------------------------
        private int CalculateCostForCurrentPath()
        {
            if(CurrentPath == null || CurrentPath.Length <= 1)
            {
                return 0;
            }

            int toReturn = 0;
            for(int i = 1; i < CurrentPath.Length; i++)
            {
                BoardTile boardTile = _game.board.GetTile(CurrentPath[i]);
                toReturn += boardTile.GetMoveCostForUnit(unit);
            }

            return toReturn;
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