////////////////////////////////////////////////////////////////////////////////
//
//  UnitActionManager (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 01/26/2020
//
////////////////////////////////////////////////////////////////////////////////

using Tofunaut.GridStrategy.Game.UI;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class UnitActionManager : UIWorldInteractionPanel.IListener
    {
        private readonly Game _game;
        
        private Unit _selectedUnit;
        private UnitPathSelectionView _pathSelectionView;
        private BoardTile _prevBoardTile;

        // --------------------------------------------------------------------------------------------
        public UnitActionManager(Game game)
        {
            _game = game;

            _pathSelectionView = new UnitPathSelectionView(_game);
        }

        #region UIWorldInteractionPanel.IListener

        // --------------------------------------------------------------------------------------------
        public void OnDragBoard(Vector2 prevDragPosition, Vector2 dragDelta) { }

        // --------------------------------------------------------------------------------------------
        public void OnDragFromUnitView(UnitView unitView, Vector2 prevDragPosition, Vector2 dragDelta)
        {
            if(_selectedUnit != null && unitView.Unit != _selectedUnit)
            {
                return;
            }

            _selectedUnit = unitView.Unit;
            _pathSelectionView.unit = _selectedUnit;

            if (!_selectedUnit.HasMoved)
            {
                if(!_pathSelectionView.IsBuilt)
                {
                    _pathSelectionView.Render(AppManager.Transform);
                }

                if(_game.board.RaycastToPlane(_game.gameCamera.ScreenPointToRay(prevDragPosition + dragDelta), out Vector3 worldPos))
                {
                    BoardTile boardTile = _game.board.GetBoardTileAtPosition(worldPos);
                    if (boardTile != null && boardTile != _prevBoardTile)
                    {
                        _prevBoardTile = boardTile;
                        _pathSelectionView.PathTo(_prevBoardTile);
                    }
                }
            }
            else if(!_selectedUnit.HasUsedSkill)
            {
                // show target skill view
            }
        }

        // --------------------------------------------------------------------------------------------
        public void OnPointerDownOverBoard(BoardTileView boardTileView) { }

        // --------------------------------------------------------------------------------------------
        public void OnReleasedBoard(Vector2 releasePosition)
        {
            if(_pathSelectionView.IsBuilt)
            {
                _pathSelectionView.Destroy();
            }

            _selectedUnit = null;
        }

        // --------------------------------------------------------------------------------------------
        public void OnSelectedUnitView(UnitView unitView) { }

        #endregion UIWorldInteractionPanel.IListener
    }
}