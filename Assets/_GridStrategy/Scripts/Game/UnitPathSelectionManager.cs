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
        private const float SelectBoardTileMaxDrag = 40f;

        public event EventHandler<PathEventArgs> OnPathSelected;
        public event EventHandler<BoardTileEventArgs> OnBoardTileSelected;
        public event EventHandler<UnitEventArgs> OnUnitSelected;

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
        private BoardTile _endTile;
        private Vector2 _dragBoardDelta;

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

        // --------------------------------------------------------------------------------------------
        public void ClearSelection()
        {
            if (_pathView.IsBuilt)
            {
                _pathView.Destroy();
            }
            _currentPath = null;
            _endTile = null;
            _dragBoardDelta = Vector2.zero;
        }

        // --------------------------------------------------------------------------------------------
        private void UpdatePathColor()
        {
            bool isEnemy = false;
            foreach (Unit occupant in _endTile.Occupants)
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

        // --------------------------------------------------------------------------------------------
        private BoardTileView RaycastForBoardTileView(Vector2 screenPosition)
        {
            BoardTileView toReturn = null;

            Ray ray = _game.gameCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                UnitView hitUnitView = hit.collider.GetComponentInParent<UnitView>();
                if (hitUnitView != null)
                {
                    BoardTileView.TryGetView(hitUnitView.Unit.BoardTile, out toReturn);
                }
                else
                {
                    toReturn = hit.collider.GetComponentInParent<BoardTileView>();
                }
            }

            return toReturn;
        }

        #region UIWorldInteractionPanel.IListener

        // --------------------------------------------------------------------------------------------
        public void OnDragBoard(Vector2 prevDragPosition, Vector2 dragDelta)
        {
            if (!_enabled)
            {
                return;
            }

            _dragBoardDelta += dragDelta;
        }

        // --------------------------------------------------------------------------------------------
        public void OnDragFromUnitView(UnitView unitView, Vector2 prevDragPosition, Vector2 dragDelta)
        {
            if (!_enabled)
            {
                return;
            }

            _draggingFrom = unitView;
            if (_draggingFrom.Unit.HasMoved)
            {
                // don't show if the unit has already moved
                return;
            }

            if(_draggingFrom.Unit.Owner.playerIndex != _game.CurrentPlayer.playerIndex)
            {
                // don't show when the unit is not owned by the current player
                return;
            }

            if (_currentPath == null)
            {
                _currentPath = new[] { unitView.Unit.BoardTile.Coord };
            }

            BoardTileView boardTileView = RaycastForBoardTileView(prevDragPosition + dragDelta);
            if (boardTileView == null)
            {
                return;
            }

            IntVector2 hitCoord = boardTileView.BoardTile.Coord;

            // check:
            // 1) the hitCoord is not null
            // 2) the hitCoord is different from the last coord on the current path
            if (hitCoord != null && hitCoord != _currentPath[_currentPath.Length - 1])
            {
                int pathCost = _game.board.CalculatePathCost(_currentPath, _draggingFrom.Unit);
                int costOfHitTile = _game.board[hitCoord.x, hitCoord.y].GetMoveCostForUnit(_draggingFrom.Unit);

                if (_currentPath.Length == 1)
                {
                    if (pathCost + costOfHitTile > _draggingFrom.Unit.MoveRange)
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
                else if (!Board.DoesPathContainCoord(_currentPath, hitCoord))
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

            if (_currentPath.Length > 1)
            {
                if (!_pathView.IsBuilt)
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
                if (_pathView.IsBuilt)
                {
                    _pathView.Destroy();
                }
            }
        }

        // --------------------------------------------------------------------------------------------
        public void OnSelectedUnitView(UnitView unitView)
        {
            if (!_enabled)
            {
                return;
            }

            OnUnitSelected.Invoke(this, new UnitEventArgs(unitView));
        }

        // --------------------------------------------------------------------------------------------
        public void OnReleasedBoard(Vector2 releasePosition)
        {
            if (!_enabled)
            {
                return;
            }

            _endTile = RaycastForBoardTileView(releasePosition)?.BoardTile;
            if(_endTile == null)
            {
                return;
            }

            if(_currentPath != null && _currentPath.Length > 1)
            {

                OnPathSelected?.Invoke(this, new PathEventArgs(_draggingFrom, _currentPath));
            }
            else if ((_currentPath == null || _currentPath.Length <= 1) && _dragBoardDelta.magnitude < SelectBoardTileMaxDrag)
            {
                if (BoardTileView.TryGetView(_endTile, out BoardTileView boardTileView))
                {
                    OnBoardTileSelected?.Invoke(this, new BoardTileEventArgs(boardTileView));
                }
                else
                {
                    OnBoardTileSelected?.Invoke(this, null);
                    Debug.LogWarning($"the coord {_currentPath[0].ToString()} isn't associated with a BoardTileView");
                }

                ClearSelection();
                return;
            }

            _dragBoardDelta = Vector2.zero;
        }

        // --------------------------------------------------------------------------------------------
        public void OnPointerDownOverBoard(BoardTileView boardTileView)
        {
            if (!_enabled)
            {
                return;
            }

            _dragBoardDelta = Vector2.zero;
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

        // --------------------------------------------------------------------------------------------
        public class BoardTileEventArgs : EventArgs
        {
            public readonly BoardTileView boardTileView;

            // --------------------------------------------------------------------------------------------
            public BoardTileEventArgs(BoardTileView boardTileView)
            {
                this.boardTileView = boardTileView;
            }
        }

        // --------------------------------------------------------------------------------------------
        public class UnitEventArgs : EventArgs
        {
            public readonly UnitView unitView;

            public UnitEventArgs(UnitView unitView)
            {
                this.unitView = unitView;
            }
        }
    }
}
