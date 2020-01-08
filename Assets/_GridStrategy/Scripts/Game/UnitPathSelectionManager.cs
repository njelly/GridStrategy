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

        private event EventHandler<PathEventArgs> OnPathSelected;

        private readonly Game _game;

        private UnitView _draggingFrom;
        private IntVector2[] _currentPath;
        private SharpLineRenderer _pathView;

        // --------------------------------------------------------------------------------------------
        public UnitPathSelectionManager(Game game)
        {
            _game = game;

            _pathView = new SharpLineRenderer("PathSelectionView", new[] { AppManager.AssetManager.Get<Material>(AssetPaths.Materials.WaypointPath) }, new Vector3[0]);
            _pathView.TexOffsetAnimVelocity = new Vector2(-1f, 0f);
            _pathView.TextureMode = LineTextureMode.Tile;
            _pathView.UseWorldSpace = true;
        }

        #region UIWorldInteractionPanel.IListener

        // --------------------------------------------------------------------------------------------
        public void OnDragBoard(Vector2 prevDragPosition, Vector2 dragDelta) { }

        // --------------------------------------------------------------------------------------------
        public void OnDragFromUnitView(UnitView unitView, Vector2 prevDragPosition, Vector2 dragDelta)
        {
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
            // 3) the hitCoord is adjacent to the last coord on the current path
            if(hitCoord != null && hitCoord != _currentPath[_currentPath.Length - 1] && (hitCoord - _currentPath[_currentPath.Length - 1]).ManhattanDistance == 1)
            {
                if (_currentPath.Length == 1)
                {
                    // always add the hitCoord when it is only the second coord in the path
                    _currentPath = new[] { _currentPath[0], hitCoord };
                }
                else if(!DoesCurrentPathContainCoord(hitCoord))
                {
                    // if the hitCoord is not covered by the current path, add it
                    List<IntVector2> pathAsList = new List<IntVector2>(_currentPath);
                    pathAsList.Add(hitCoord);
                    _currentPath = pathAsList.ToArray();
                }
                else
                {
                    // the path already contains this point, so backtrack to it
                    BacktrackTo(hitCoord);
                }
            }

            SimplifyPath();

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
            OnPathSelected?.Invoke(this, new PathEventArgs(_draggingFrom, _currentPath));

            // TODO: probably don't destroy the path view immediately
            if (_pathView.IsBuilt)
            {
                _pathView.Destroy();
            }
            _currentPath = null;
        }

        #endregion

        // --------------------------------------------------------------------------------------------
        private void SimplifyPath()
        {
            bool removePoint = false;
            int i;
            for(i = 1; i < _currentPath.Length; i++)
            {
                if(_currentPath[i].Equals(_currentPath[i - 1]))
                {
                    // remove equal coords
                    removePoint = true;
                    break;
                }

                if(i < _currentPath.Length - 1 && _currentPath[i].IsCollinear(_currentPath[i - 1], _currentPath[i + 1]))
                {
                    // remove coords that are collinear with the previous and next coords
                    removePoint = true;
                    break;
                }
            }

            // keep attempting to simplify the path until there are no more points to be removed
            if (removePoint)
            {
                List<IntVector2> pathAsList = new List<IntVector2>(_currentPath);
                pathAsList.RemoveAt(i);
                _currentPath = pathAsList.ToArray();

                SimplifyPath();
            }
        }

        // --------------------------------------------------------------------------------------------
        private void BacktrackTo(IntVector2 coord)
        {
            // edge case when backtracking to the first square
            if (coord.Equals(_currentPath[0]))
            {
                _currentPath = new[] { _currentPath[0] };
                return;
            }

            List<IntVector2> pathAsList = new List<IntVector2>();
            pathAsList.Add(_currentPath[0]);

            for(int i = 1; i < _currentPath.Length; i++)
            {
                if (coord.Equals(_currentPath[i]) || coord.IsCollinearAndBetween(_currentPath[i - 1], _currentPath[i]))
                {
                    pathAsList.Add(coord);
                    break;
                }

                pathAsList.Add(_currentPath[i]);
            }

            _currentPath = pathAsList.ToArray();
        }

        // --------------------------------------------------------------------------------------------
        private bool DoesCurrentPathContainCoord(IntVector2 coord)
        {
            if (_currentPath == null)
            {
                return false;
            }

            if(_currentPath.Length <= 0)
            {
                return false;
            }

            if(coord.Equals(_currentPath[0]))
            {
                return true;
            }

            bool toReturn = false;
            for (int i = 1; i < _currentPath.Length; i++)
            {
                toReturn |= coord.Equals(_currentPath[i]) || coord.IsCollinearAndBetween(_currentPath[i - 1], _currentPath[i]);
            }

            return toReturn;
        }

        // --------------------------------------------------------------------------------------------
        private void DebugPrintCurrentPath()
        {
            if(_currentPath == null)
            {
                Debug.Log("NULL PATH");
                return;
            }
            if(_currentPath.Length == 0)
            {
                Debug.Log("EMPTY PATH");
                return;
            }

            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < _currentPath.Length; i++)
            {
                sb.Append(_currentPath[i].ToString());
                sb.Append(" | ");
            }
            Debug.Log(sb.ToString());
        }

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
