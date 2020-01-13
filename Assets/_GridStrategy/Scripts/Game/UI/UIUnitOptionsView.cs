//////////////////////////////////////////////////////////////////////////////
//
//  UIUnitOptionsView (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 01/13/2020
//
////////////////////////////////////////////////////////////////////////////////

using System;
using Tofunaut.GridStrategy.UI;
using Tofunaut.SharpUnity.UI;
using Tofunaut.SharpUnity.UI.Behaviour;
using Tofunaut.UnityUtils;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game.UI
{
    // --------------------------------------------------------------------------------------------
    public class UIUnitOptionsView : UIGridStrategyView, Updater.IUpdateable
    {
        // --------------------------------------------------------------------------------------------
        public interface IListener
        {
            void OnUseSkillClicked(Unit unit);
        }

        private readonly IListener _listener;
        private readonly GameCamera _gameCamera;

        private Unit _following;
        private SharpUIVerticalLayout _container;

        // --------------------------------------------------------------------------------------------
        public UIUnitOptionsView(IListener listener, GameCamera gameCamera)
        {
            _listener = listener;
            _gameCamera = gameCamera;
        }

        // --------------------------------------------------------------------------------------------
        protected override SharpUIBase BuildMainPanel()
        {
            _container = new SharpUIVerticalLayout("UIUnitOptionsView");
            _container.SetFitSize();

            SharpUIImage background = new SharpUIImage($"{_container.Name}_bg", null);
            background.Color = new Color(0f, 0f, 0f, 0.5f);
            _container.AddChild(background, true);

            _container.AddChild(new UIUnitOptionButton("UseSkillButton", "Use Skill", () =>
            {
                _listener.OnUseSkillClicked(_following);
            }));
            _container.AddChild(new UIUnitOptionButton("Close", "Close", () =>
            {
                Hide();
            }));

            return _container;
        }

        // --------------------------------------------------------------------------------------------
        public override void Show()
        {
            base.Show();

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
            _container.RectTransform.anchoredPosition = new Vector2(0, 100) + UIMainCanvas.Instance.GetCanvasPositionForGameObject(_following.GameObject, _gameCamera.UnityCamera);
        }

        // --------------------------------------------------------------------------------------------
        private class UIUnitOptionButton : SharpUIImage
        {
            public Vector2 Size => new Vector2(200, 80);

            // --------------------------------------------------------------------------------------------
            public UIUnitOptionButton(string name, string caption, Action onClick) : base(name, null)
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

                SubscribeToEvent(EEventType.PointerClick, (object sender, EventSystemEventArgs e) =>
                {
                    onClick?.Invoke();
                });
            }
        }
    }
}
