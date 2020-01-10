////////////////////////////////////////////////////////////////////////////////
//
//  UnitPathSelectionManager (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 01/07/2020
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using TofuCore;
using Tofunaut.GridStrategy.Game.UI;
using Tofunaut.SharpUnity;
using Tofunaut.UnityUtils;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class UnitPathSelectionManager : UIWorldInteractionPanel.IListener
    {
        private const float PathViewHeight = 1.5f;

        public event EventHandler<PathEventArgs> OnPathSelected;

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                if (!_enabled)
                {
                    ClearSelection();
                }
            }
        }

        private readonly Game _game;

        private UnitView _draggingFrom;
        private IntVector2[] _currentPath;
        private SharpLineRenderer _pathView;
        private bool _enabled;
        private bool _hitEnemy;
        private BoardTile _endTile;

        // --------------------------------------------------------------------------------------------
        public UnitPathSelectionManager(Game game)
        {
            _game = game;

            _pathView = new SharpLineRenderer("PathSelectionView", new[] { AppManager.AssetManager.Get<Material>(AssetPaths.Materials.WaypointPath) }, new Vector3[0]);
            _pathView.TexOffsetAnimVelocity = new Vector2(-1f, 0f);
            _pathView.TextureMode = LineTextureMode.Tile;
            _pathView.UseWorldSpace = true;

            _enabled = true;
        }

        public void ClearSelection()
        {
            if (_pathView.IsBuilt)
            {
                _pathView.Destroy();
            }
            _currentPath = null;
            _hitEnemy = false;
            _endTile = null;
        }

        private void UpdatePathColor()
        {
            bool isEnemy = false;
            foreach(Unit occupant in _endTile.Occupants)
            {
                if (!_draggingFrom.Unit.IsAllyOf(occupant))
                {
                    isEnemy = true;
                    break;
                }
            }

            if (isEnemy)
            {
                _pathView.StartColor = Color.red;
                _pathView.EndColor = Color.red;
            }
            else
            {
                _pathView.StartColor = Color.white;
                _pathView.EndColor = Color.white;
            }
        }

        #region UIWorldInteractionPanel.IListener

        // --------------------------------------------------------------------------------------------
        public void OnDragBoard(Vector2 prevDragPosition, Vector2 dragDelta) { }

        // --------------------------------------------------------------------------------------------
        public void OnDragFromUnitView(UnitView unitView, Vector2 prevDragPosition, Vector2 dragDelta)
        {
            if(!_enabled)
            {
                return;
            }

            if(_currentPath == null)
            {
                _currentPath = new[] { unitView.Unit.BoardTile.Coord };
            }

            _draggingFrom = unitView;

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

            // check:
            // 1) the hitCoord is not null
            // 2) the hitCoord is different from the last coord on the current path
            if(hitCoord != null && hitCoord != _currentPath[_currentPath.Length - 1])
            {
                int pathCost = _game.board.CalculatePathCost(_currentPath, _draggingFrom.Unit);
                int costOfHitTile = _game.board[hitCoord.x, hitCoord.y].GetMoveCostForUnit(_draggingFrom.Unit);

                if (_currentPath.Length == 1)
                {
                    if(pathCost + costOfHitTile > _draggingFrom.Unit.MoveRange)
                    {
                        // return immediately if this would create a path that is too expensive
                        return;
                    }

                    List<IntVector2> potentialPath = new List<IntVector2>(_currentPath);
                    potentialPath.Add(hitCoord);
                    if (!Board.IsPathValid(potentialPath.ToArray()))
                    {
                        // return if the path would not be valid by adding the hitCoord
                        return;
                    }

                    // always add the hitCoord when it is only the second coord in the path
                    _currentPath = new[] { _currentPath[0], hitCoord };
                }
                else if(!Board.DoesPathContainCoord(_currentPath, hitCoord))
                {
                    //if the hitCoord is not covered by the current path

                    if (pathCost + costOfHitTile > _draggingFrom.Unit.MoveRange)
                    {
                        // return immediately if this would create a path that is too expensive
                        return;
                    }

                    if ((hitCoord - _currentPath[_currentPath.Length - 1]).ManhattanDistance == 1)
                    {
                        // if it is adjacent to the last path point, add it
                        // if its collinear, this will be cleaned up in SimplifyPath()
                        List<IntVector2> pathAsList = new List<IntVector2>(_currentPath);
                        pathAsList.Add(hitCoord);
                        _currentPath = pathAsList.ToArray();
                    }
                }
                else
                {
                    // the path already contains this point, so backtrack to it
                    _currentPath = Board.BacktrackTo(_currentPath, hitCoord);
                }

                _endTile = _game.board.GetTile(hitCoord);
                UpdatePathColor();
            }

            _currentPath = Board.SimplifyPath(_currentPath);

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

        // --------------------------------------------------------------------------------------------
        public void OnSelectedUnitView(UnitView unitView) { }

        // --------------------------------------------------------------------------------------------
        public void OnReleasedBoard(Vector2 releasePosition)
        {
            if (!_enabled)
            {
                return;
            }
            
            if (_currentPath == null)
            {
                return;
            }

            if (_currentPath.Length <= 1)
            {
                ClearSelection();
                return;
            }

            OnPathSelected?.Invoke(this, new PathEventArgs(_draggingFrom, _currentPath));
        }

        #endregion

        // --------------------------------------------------------------------------------------------
        public class PathEventArgs : EventArgs
        {
            public readonly IntVector2[] path;
            public readonly UnitView unitView;

            // --------------------------------------------------------------------------------------------
            public PathEventArgs(UnitView unitView, IntVector2[] path)
            {
                this.unitView = unitView;
                this.path = path;
            }
        }
    }
}
