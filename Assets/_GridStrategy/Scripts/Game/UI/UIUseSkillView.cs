//////////////////////////////////////////////////////////////////////////////
//
//  UIUnitOptionsView (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 01/13/2020
//
////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using TofuCore;
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
    public class UIUseSkillView : UIGridStrategyView, Updater.IUpdateable
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
        private bool _selectingDirection;
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

            toReturn.SubscribeToEvent(EEventType.PointerDown, (object sender, EventSystemEventArgs e) =>
            {
                Hide();
            });
            toReturn.SubscribeToEvent(EEventType.Drag, (object sender, EventSystemEventArgs e) =>
            {
                if(!_selectingDirection)
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
                    if (_selectingDirection && _selectedBoardTile != null)
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

            _useSkillButton = new UIUnitOptionButton("UseSkillButton", "Use Skill");
            toReturn.AddChild(_useSkillButton);

            _useSkillButton.SubscribeToEvent(EEventType.PointerDown, (object sender, EventSystemEventArgs e) =>
            {
                _selectingDirection = true;
                _useSkillButton.Destroy();

                PointerEventData pointerEventData = e.eventData as PointerEventData;
                _game.board.RaycastToPlane(pointerEventData.position, out _startDragWorldPos);
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

            _selectingDirection = false;
            _selectedBoardTile = null;

            _game.board.HighlightBoardTilesForUseSkill(_following.Skill);

            UpdatePosition();

            Updater.Instance.Add(this);
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

            Updater.Instance.Remove(this);
        }

        // --------------------------------------------------------------------------------------------
        public void FollowUnit(Unit unit)
        {
            _following = unit;
            _targetableTiles = unit.Skill.GetTargetableTiles();
        }

        // --------------------------------------------------------------------------------------------
        public void Update(float deltaTime)
        {
            if(_following == null || !_following.IsBuilt)
            {
                Hide();
            }

            UpdatePosition();
        }

        // --------------------------------------------------------------------------------------------
        private void UpdatePosition()
        {
            if(!_selectingDirection)
            {
                _useSkillButton.RectTransform.anchoredPosition = new Vector2(0, 100)
                    + UIMainCanvas.Instance.GetCanvasPositionForGameObject(_following.GameObject, _game.gameCamera.UnityCamera);
            }
        }

        // --------------------------------------------------------------------------------------------
        private class UIUnitOptionButton : SharpUIImage
        {
            public Vector2 Size => new Vector2(200, 80);

            // --------------------------------------------------------------------------------------------
            public UIUnitOptionButton(string name, string caption) : base(name, null)
            {
                SetFixedSize(Size);
                Color = new Color(0.25f, 0.25f, 0.25f, 1f);
                margin = new RectOffset(10, 10, 10, 10);

                SharpUITextMeshPro label = new SharpUITextMeshPro($"{name}_label", caption);
                label.SetFillSize();
                label.AutoSizeFont();
                label.Font = AppManager.AssetManager.Get<TMPro.TMP_FontAsset>(AssetPaths.Fonts.GravityBook);
                label.Color = Color.black;
                label.TextAlignment = TMPro.TextAlignmentOptions.Center;
                label.Color = Color.white;
                AddChild(label);
            }
        }
    }
}