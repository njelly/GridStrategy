//////////////////////////////////////////////////////////////////////////////
//
//  UIHUDManager (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 01/05/2020
//
////////////////////////////////////////////////////////////////////////////////

using System;
using Tofunaut.Animation;
using Tofunaut.SharpUnity.UI;
using UnityEngine;

namespace Tofunaut.GridStrategy.UI
{
    public class UIBeginTurnBanner : GridStrategyUIView
    {
        private const int BannerHeight = 300;
        private Color BackgroundStartColor => new Color(1f, 1f, 1f, 0f);
        private Color BackgroundEndColor => new Color(1f, 1f, 1f, 0.3f);
        private const float AnimateInTime = 0.5f;
        private const float AnimateHoldTime = 3f;
        private const float AnimateOutTime = 0.5f;

        private SharpUIImage _bannerBackground;
        private SharpUITextMeshPro _bannerLabel;
        private string _playerName;
        private TofuAnimation _anim;

        protected override SharpUIBase BuildMainPanel()
        {
            _bannerBackground = new SharpUIImage("UIBeginTurnBanner", null);
            _bannerBackground.SetFillSize(EAxis.X, 1f);
            _bannerBackground.SetFixedSize(EAxis.Y, BannerHeight);
            _bannerBackground.Color = new Color(1f, 1f, 1f, 0.3f);
            _bannerBackground.alignment = EAlignment.MiddleCenter;

            _bannerLabel = new SharpUITextMeshPro($"{_bannerBackground.name}_Label", _playerName);
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

            // temp animation to make sure everything is aligned right
            _anim = new TofuAnimation()
                .Wait(AnimateHoldTime)
                .Then()
                .Execute(() =>
                {
                    _anim = null;
                    Hide();
                })
                .Play();

            //new TofuAnimation()
            //    .Value01(AnimateInTime, EEaseType.Linear, (float newValue) =>
            //    {
            //        _bannerBackground.Color = Color.Lerp(BackgroundStartColor, BackgroundEndColor, newValue);
            //    })
            //    .Value01(AnimateInTime, EEaseType.EaseInExpo, (float newValue) =>
            //    {
            //        _bannerLabel.LocalPosition
            //    })
        }
    }
}