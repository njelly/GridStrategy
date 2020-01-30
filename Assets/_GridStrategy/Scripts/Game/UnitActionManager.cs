////////////////////////////////////////////////////////////////////////////////
//
//  UnitActionManager (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 01/26/2020
//
////////////////////////////////////////////////////////////////////////////////

using TofuCore;
using Tofunaut.GridStrategy.Game.UI;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class UnitActionManager : UIWorldInteractionPanel.IListener
    {
        public interface IListener
        {
            void OnPathSelected(Unit unit, IntVector2[] path);
            void OnSkillTargetSelected(Unit unit, Unit.EFacing facing, BoardTile target);
        }

        private readonly IListener _listener;
        private readonly Game _game;
        private readonly UnitPathSelectionView _pathSelectionView;
        private readonly UnitUseSkillView _useSkillView;

        private Unit _selectedUnit;
        private BoardTile _prevBoardTile;

        // --------------------------------------------------------------------------------------------
        public UnitActionManager(IListener listener, Game game)
        {
            _listener = listener;
            _game = game;

            _pathSelectionView = new UnitPathSelectionView(_game);
            _useSkillView = new UnitUseSkillView(_game);
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
            _useSkillView.unit = _selectedUnit;

            if (_game.board.RaycastToPlane(_game.gameCamera.ScreenPointToRay(prevDragPosition + dragDelta), out Vector3 worldPos))
            {
                BoardTile boardTile = _game.board.GetBoardTileAtPosition(worldPos);
                if (boardTile != null && boardTile != _prevBoardTile)
                {
                    _prevBoardTile = boardTile;
                }
            }

            if (!_selectedUnit.HasMoved)
            {
                if(!_pathSelectionView.IsBuilt)
                {
                    _pathSelectionView.Render(AppManager.Transform);
                }
                if(_useSkillView.IsBuilt)
                {
                    _useSkillView.Destroy();
                }

                _pathSelectionView.PathTo(_prevBoardTile);
            }
            else if(!_selectedUnit.HasUsedSkill)
            {
                if(!_useSkillView.IsBuilt)
                {
                    _useSkillView.Render(AppManager.Transform);
                }
                if(_pathSelectionView.IsBuilt)
                {
                    _pathSelectionView.Destroy();
                }

                _useSkillView.TargetTowardTile(_prevBoardTile);
            }
        }

        // --------------------------------------------------------------------------------------------
        public void OnPointerDownOverBoard(BoardTileView boardTileView) { }

        // --------------------------------------------------------------------------------------------
        public void OnReleasedBoard(Vector2 releasePosition)
        {
            if (_pathSelectionView.IsBuilt)
            {
                _listener.OnPathSelected(_selectedUnit, _pathSelectionView.CurrentPath);
                _pathSelectionView.Destroy();
            }
            if (_useSkillView.IsBuilt)
            {
                _listener.OnSkillTargetSelected(_selectedUnit, _useSkillView.CurrentFacing, _prevBoardTile);
                _useSkillView.Destroy();
            }

            _selectedUnit = null;


        }

        // --------------------------------------------------------------------------------------------
        public void OnSelectedUnitView(UnitView unitView) { }

        #endregion UIWorldInteractionPanel.IListener
    }
}