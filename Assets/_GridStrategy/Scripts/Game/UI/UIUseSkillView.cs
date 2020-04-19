////////////////////////////////////////////////////////////////////////////////
//
//  UIUnitOptionsView (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 01/13/2020
//
////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using Tofunaut.Core;
using Tofunaut.GridStrategy.UI;
using Tofunaut.SharpUnity;
using Tofunaut.SharpUnity.UI;
using Tofunaut.SharpUnity.UI.Behaviour;
using Tofunaut.UnityUtils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tofunaut.GridStrategy.Game.UI
{
    // --------------------------------------------------------------------------------------------
    public class UIUseSkillView : UIGridStrategyView
    {
        // --------------------------------------------------------------------------------------------
        public interface IListener
        {
            void OnUseSkillConfirmed(Unit unit, Unit.EFacing facing, IntVector2 targetCoord);
        }

        public static class State
        {
            public const string Prompt = "prompt";
            public const string ChooseFacing = "facing";
        }

        private readonly IListener _listener;
        private readonly Game _game;

        private Unit _following;
        private SharpUIImage _useSkillButton;
        private SharpSprite _facingArrow;
        private Unit.EFacing _currentFacing;
        private Vector3 _startDragWorldPos;
        private BoardTile _selectedBoardTile;
        private List<BoardTile> _targetableTiles;

        // --------------------------------------------------------------------------------------------
        public UIUseSkillView(IListener listener, Game game) : base (UIPriorities.WorldUI)
        {
            _listener = listener;
            _game = game;

            _facingArrow = new SharpSprite("FacingArrow", AppManager.AssetManager.Get<Sprite>(AssetPaths.Sprites.FacingArrow));
            _facingArrow.LocalScale = Vector3.one * 4f;
        }

        // --------------------------------------------------------------------------------------------
        protected override SharpUIBase BuildMainPanel()
        {
            SharpUINonDrawingGraphic toReturn = new SharpUINonDrawingGraphic("UIUnitOptionsView");
            toReturn.SetFillSize();

            toReturn.SubscribeToEvent(EEventType.Drag, (object sender, EventSystemEventArgs e) =>
            {
                if(_following == null)
                {
                    return;
                }

                PointerEventData pointerEventData = e.eventData as PointerEventData;

                if(_game.board.RaycastToPlane(pointerEventData.position, out Vector3 dragWorldPos))
                {
                    BoardTile draggingTile = _game.board.GetBoardTileAtPosition(dragWorldPos);
                    if(draggingTile != null && _targetableTiles.Contains(draggingTile))
                    {
                        _currentFacing = Unit.VectorToFacing(draggingTile.Transform.position - _following.BoardTile.Transform.position);
                        _selectedBoardTile = draggingTile;
                        _following.BoardTile.AddChild(_facingArrow);

                        Quaternion facingRot = Unit.FacingToRotation(_currentFacing);
                        _facingArrow.LocalRotation = Quaternion.Euler(90f, facingRot.eulerAngles.y - 90f, 0f);
                        _facingArrow.LocalPosition = Vector3.up + (Quaternion.Euler(0f, facingRot.eulerAngles.y - 90f, 0f) * (Vector3.right * 2f));
                    }
                    else
                    {
                        if(_facingArrow.IsBuilt)
                        {
                            _facingArrow.Destroy();
                        }

                        _selectedBoardTile = null;
                        _currentFacing = _following.Facing;
                    }
                }
            });
            toReturn.SubscribeToEvent(EEventType.Drop, (object sender, EventSystemEventArgs e) =>
            {
                if(_facingArrow.IsBuilt)
                {
                    if (_selectedBoardTile != null)
                    {
                        _listener.OnUseSkillConfirmed(_following, _currentFacing, _selectedBoardTile.Coord);
                    }

                    _facingArrow.Destroy();
                }
                else
                {
                    Hide();
                }
            });

            return toReturn;
        }

        // --------------------------------------------------------------------------------------------
        public override void Show()
        {
            if(_following == null || _following.HasUsedSkill || _targetableTiles == null || _targetableTiles.Count <= 0)
            {
                return;
            }

            base.Show();

            _selectedBoardTile = null;

            _game.board.HighlightBoardTilesForUseSkill(_following.Skill);
        }

        // --------------------------------------------------------------------------------------------
        public override void Hide()
        {
            base.Hide();

            if(_facingArrow?.IsBuilt ?? false)
            {
                _facingArrow.Destroy();
            }

            _game.board.ClearAllBoardTileHighlights();
        }

        // --------------------------------------------------------------------------------------------
        public void FollowUnit(Unit unit)
        {
            _following = unit;
            _targetableTiles = unit.Skill.GetTargetableTiles();
        }
    }
}