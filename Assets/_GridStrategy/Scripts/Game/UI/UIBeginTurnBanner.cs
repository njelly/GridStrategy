//////////////////////////////////////////////////////////////////////////////
//
//  UIBeginTurnBanner (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 01/05/2020
//
////////////////////////////////////////////////////////////////////////////////

using Tofunaut.Animation;
using Tofunaut.SharpUnity.UI;
using UnityEngine;

namespace Tofunaut.GridStrategy.UI
{
    public class UIBeginTurnBanner : GridStrategyUIView
    {
        private const int BannerHeight = 300;
        private Color BackgroundStartColor => new Color(1f, 1f, 1f, 0f);
        private Color BackgroundEndColor => new Color(1f, 1f, 1f, 0.4f);
        private const float AnimateInTime = 1f;
        private const float AnimateHoldTime = 2f;
        private const float AnimateOutTime = 0.5f;

        private SharpUIImage _bannerBackground;
        private SharpUITextMeshPro _bannerLabel;
        private string _playerName;
        private TofuAnimation _anim;

        // always render behind the UIWorldIteractionManager so that this doesn't block input
        public UIBeginTurnBanner() : base(UIPriorities.UIWorldInteractionManager - 1) { }

        protected override SharpUIBase BuildMainPanel()
        {
            _bannerBackground = new SharpUIImage("UIBeginTurnBanner", null);
            _bannerBackground.SetFillSize(EAxis.X, 1f);
            _bannerBackground.SetFixedSize(EAxis.Y, BannerHeight);
            _bannerBackground.Color = new Color(1f, 1f, 1f, 0.3f);
            _bannerBackground.alignment = EAlignment.MiddleCenter;

            _bannerLabel = new SharpUITextMeshPro($"{_bannerBackground.name}_Label", _playerName);
            _bannerLabel.Font = AppManager.AssetManager.Get<TMPro.TMP_FontAsset>(AssetPaths.Fonts.GravityLightItalic);
            _bannerLabel.SetFillSize();
            _bannerLabel.TextAlignment = TMPro.TextAlignmentOptions.Center;
            _bannerLabel.AutoSizeFont();
            _bannerLabel.Color = Color.black;
            _bannerLabel.alignment = EAlignment.MiddleCenter;
            _bannerBackground.AddChild(_bannerLabel);

            return _bannerBackground;
        }

        public override void Show()
        {
            base.Show();

            Animate();
        }

        public override void Hide()
        {
            base.Hide();

            _anim?.Stop();
        }

        public void SetPlayerName(string playerName)
        {
            _playerName = playerName;
        }

        private void Animate()
        {
            _anim?.Stop();

            Vector2 startLabelPos = new Vector2(_bannerBackground.Width, 0f);
            Vector2 holdLabelPos = new Vector2(0f, 0f);
            Vector2 endLabelPos = new Vector2(-_bannerBackground.Width, 0f);

            _anim = new TofuAnimation()
                .Value01(AnimateInTime, EEaseType.Linear, (float newValue) =>
                {
                    _bannerBackground.Color = Color.Lerp(BackgroundStartColor, BackgroundEndColor, newValue);
                })
                .Value01(AnimateInTime, EEaseType.EaseInOutCirc, (float newValue) =>
                {
                    _bannerLabel.LocalPosition = Vector3.LerpUnclamped(startLabelPos, holdLabelPos, newValue);
                })
                .Then()
                .Wait(AnimateHoldTime)
                .Then()
                .Value01(AnimateInTime, EEaseType.Linear, (float newValue) =>
                {
                    _bannerBackground.Color = Color.Lerp(BackgroundEndColor, BackgroundStartColor, newValue);
                })
                .Value01(AnimateOutTime, EEaseType.EaseInOutCirc, (float newValue) =>
                {
                    _bannerLabel.LocalPosition = Vector3.LerpUnclamped(holdLabelPos, endLabelPos, newValue);
                })
                .Then()
                .Execute(() =>
                {
                    Hide();
                })
                .Play();
        }
    }
}