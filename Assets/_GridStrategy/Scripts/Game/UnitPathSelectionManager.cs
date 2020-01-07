
using System;
using System.Collections.Generic;
using TofuCore;
using Tofunaut.GridStrategy.Game.UI;
using Tofunaut.SharpUnity;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    public class UnitPathSelectionManager : UIWorldInteractionPanel.IListener
    {
        private event EventHandler<PathEventArgs> OnPathSelected;

        private readonly Game _game;

        private IntVector2[] _currentPath;
        private SharpLineRenderer _pathView;

        public UnitPathSelectionManager(Game game)
        {
            _game = game;

            _pathView = new SharpLineRenderer("PathSelectionView", new[] { AppManager.AssetManager.Get<Material>(AssetPaths.Materials.WaypointPath) }, new Vector3[0]);
            _pathView.TexOffsetAnimVelocity = new Vector2(-1f, 0f);
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

            if(hitCoord != null && hitCoord != _currentPath[_currentPath.Length - 1])
            {
                if(_currentPath.Length == 1)
                {
                    _currentPath = new[] { _currentPath[0], hitCoord };
                }
                else if(hitCoord.IsCollinear(_currentPath[_currentPath.Length - 1]))
                {
                    _currentPath[_currentPath.Length - 1] = hitCoord;
                }
                else
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
                    _pathView.Positions = 
                }
            }
        }

        public void OnSelectedUnitView(UnitView unitView) { }

        public void OnReleasedBoard(Vector2 releasePosition)
        {
            throw new NotImplementedException();
        }

        #endregion

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