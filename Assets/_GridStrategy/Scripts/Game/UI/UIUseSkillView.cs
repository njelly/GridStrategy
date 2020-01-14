//////////////////////////////////////////////////////////////////////////////
//
//  UIUnitOptionsView (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 01/13/2020
//
////////////////////////////////////////////////////////////////////////////////

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
            void OnUseSkillConfirmed(Unit unit, Unit.EFacing facing);
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
        private Vector3 _startDragWorldPos;
        private Unit.EFacing _currentFacing;

        // --------------------------------------------------------------------------------------------
        public UIUseSkillView(IListener listener, Game game)
        {
            _listener = listener;
            _game = game;

            _facingArrow = new SharpSprite("FacingArrow", AppManager.AssetManager.Get<Sprite>(AssetPaths.Sprites.FacingArrow));
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

                Ray ray = _game.gameCamera.ScreenPointToRay(pointerEventData.position);
                RaycastHit[] hits = Physics.RaycastAll(ray);
                foreach (RaycastHit hit in hits)
                {
                    BoardTileView view = hit.collider.GetComponentInParent<BoardTileView>();
                    if (view != null)
                    {
                        view.BoardTile.AddChild(_facingArrow);
                        _currentFacing = Unit.VectorToFacing(view.BoardTile.GameObject.transform.position - _startDragWorldPos);
                        _facingArrow.LocalPosition = Vector3.zero;
                        _facingArrow.LocalRotation = Unit.FacingToRotation(_currentFacing);
                    }
                }
            });

            toReturn.SubscribeToEvent(EEventType.PointerUp, (object sender, EventSystemEventArgs e) =>
            {
                if(_selectingDirection)
                {
                    _listener.OnUseSkillConfirmed(_following, _currentFacing);
                }

                if(_facingArrow.IsBuilt)
                {
                    _facingArrow.Destroy();
                }
            });

            _useSkillButton = new UIUnitOptionButton("UseSkillButton", "Use Skill");
            toReturn.AddChild(_useSkillButton);

            _useSkillButton.SubscribeToEvent(EEventType.PointerDown, (object sender, EventSystemEventArgs e) =>
            {
                _selectingDirection = true;
                _useSkillButton.Destroy();

                PointerEventData pointerEventData = e.eventData as PointerEventData;

                Ray ray = _game.gameCamera.ScreenPointToRay(pointerEventData.position);
                if(_game.board.RaycastToPlane(ray, out Vector3 worldPos))
                {
                    BoardTile boardTile = _game.board.GetBoardTileAtPosition(worldPos);
                    if(boardTile != null)
                    {
                        boardTile.AddChild(_facingArrow);
                        _facingArrow.LocalRotation = (Unit.FacingToRotation(Unit.VectorToFacing(boardTile.Transform.position - _startDragWorldPos)));
                    }
                }
            });

            return toReturn;
        }

        // --------------------------------------------------------------------------------------------
        public override void Show()
        {
            base.Show();

            _selectingDirection = false;

            UpdatePosition();

            Updater.Instance.Add(this);
        }

        // --------------------------------------------------------------------------------------------
        public override void Hide()
        {
            base.Hide();

            Updater.Instance.Remove(this);
        }

        // --------------------------------------------------------------------------------------------
        public void FollowUnit(Unit unit)
        {
            _following = unit;
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