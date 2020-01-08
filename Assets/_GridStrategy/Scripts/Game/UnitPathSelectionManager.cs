////////////////////////////////////////////////////////////////////////////////
//
//  UnitPathSelectionManager (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 01/07/2020
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using TofuCore;
using Tofunaut.GridStrategy.Game.UI;
using Tofunaut.SharpUnity;
using Tofunaut.UnityUtils;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    public class UnitPathSelectionManager : UIWorldInteractionPanel.IListener
    {
        private const float PathViewHeight = 1.5f;

        private event EventHandler<PathEventArgs> OnPathSelected;

        private readonly Game _game;

        private IntVector2[] _currentPath;
        private SharpLineRenderer _pathView;

        public UnitPathSelectionManager(Game game)
        {
            _game = game;

            _pathView = new SharpLineRenderer("PathSelectionView", new[] { AppManager.AssetManager.Get<Material>(AssetPaths.Materials.WaypointPath) }, new Vector3[0]);
            _pathView.TexOffsetAnimVelocity = new Vector2(-1f, 0f);
            _pathView.TextureMode = LineTextureMode.Tile;
            _pathView.UseWorldSpace = true;
        }

        #region UIWorldInteractionPanel.IListener

        public void OnDragBoard(Vector2 prevDragPosition, Vector2 dragDelta) { }

        public void OnDragFromUnitView(UnitView unitView, Vector2 prevDragPosition, Vector2 dragDelta)
        {
            if(_currentPath == null)
            {
                _currentPath = new[] { unitView.Unit.BoardTile.Coord };
            }

            Ray ray = _game.gameCamera.ScreenPointToRay(prevDragPosition + dragDelta);
            IntVector2 hitCoord = null;
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                UnitView hitUnitView = hit.collider.GetComponentInParent<UnitView>();
                if(hitUnitView != null)
                {
                    hitCoord = hitUnitView.Unit.BoardTile.Coord;
                }
                else
                {
                    BoardTileView hitBoardTileView = hit.collider.GetComponentInParent<BoardTileView>();
                    if(hitBoardTileView != null)
                    {
                        hitCoord = hitBoardTileView.BoardTile.Coord;
                    }
                }
            }

            if(hitCoord != _currentPath[_currentPath.Length - 1])
            {
                if(_currentPath.Length == 1)
                {
                    _currentPath = new[] { _currentPath[0], hitCoord };
                }
                else if(_currentPath.Length > 1 && hitCoord.IsCollinear(_currentPath[_currentPath.Length - 2]))
                {
                    _currentPath[_currentPath.Length - 1] = hitCoord;
                }
                else if(IsNewPathPointValid(hitCoord))
                {
                    List<IntVector2> pathAsList = new List<IntVector2>(_currentPath);
                    pathAsList.Add(hitCoord);
                    _currentPath = pathAsList.ToArray();
                }
            }

            if(_currentPath.Length > 1)
            {
                if(!_pathView.IsBuilt)
                {
                    _pathView.Render(AppManager.Transform);
                }

                Vector3[] positions = new Vector3[_currentPath.Length];
                for (int i = 0; i < _currentPath.Length; i++)
                {
                    positions[i] = new Vector3(_currentPath[i].x * BoardTileView.Size, PathViewHeight, _currentPath[i].y * BoardTileView.Size);
                }

                _pathView.Positions = positions;
            }
            else 
            {
                if(_pathView.IsBuilt) 
                {
                    _pathView.Destroy();
                } 
            }
        }

        public void OnSelectedUnitView(UnitView unitView) { }

        public void OnReleasedBoard(Vector2 releasePosition)
        {
            if (_pathView.IsBuilt)
            {
                _pathView.Destroy();
            }

            _currentPath = null;
        }

        #endregion

        private bool IsNewPathPointValid(IntVector2 newPathPoint)
        {
            if (newPathPoint == null)
            {
                return false;
            }

            if (_currentPath == null)
            {
                return false;
            }

            if ((newPathPoint - _currentPath[_currentPath.Length - 1]).ManhattanDistance != 1)
            {
                return false;
            }

            bool pathAlreadyContainsPoint = false;
            for (int i = 1; i < _currentPath.Length; i++)
            {
                pathAlreadyContainsPoint |= newPathPoint.IsCollinear(_currentPath[i - 1], _currentPath[i]);
            }
            if (pathAlreadyContainsPoint)
            {
                return false;
            }

            return true;
        }

        public class PathEventArgs : EventArgs
        {
            public readonly IntVector2[] path;
            public readonly UnitView unitView;

            public PathEventArgs(UnitView unitView, IntVector2[] path)
            {
                this.unitView = unitView;
                this.path = path;
            }
        }
    }
}
