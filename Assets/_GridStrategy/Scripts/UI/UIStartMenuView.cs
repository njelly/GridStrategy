////////////////////////////////////////////////////////////////////////////////
//
//  UIStartMenuView (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/09/2019
//
////////////////////////////////////////////////////////////////////////////////

using Tofunaut.Animation;
using Tofunaut.SharpUnity.UI;
using UnityEngine;

namespace Tofunaut.GridStrategy.UI
{
    public class UIStartMenuView : SharpUIView
    {
        private const float BackgroundFadeInTime = 4f;
        private static readonly Color FadeInStartColor = new Color(0f, 0f, 0f, 1f);
        private static readonly Color FadeInEndColor = new Color(0f, 0f, 0f, 0f);

        private const float CanvasGroupFadeInDelay = 1f;
        private const float CanvasGroupFadeInTime = 3f;

        private SharpUIImage _background;
        private SharpUICanvasGroup _canvasGroup;

        public UIStartMenuView() : base(UIMainCanvas.Instance) { }

        protected override SharpUIBase BuildMainPanel()
        {
            _background = new SharpUIImage("UIStartMenuView", null);
            _background.SetFillSize();

            _canvasGroup = new SharpUICanvasGroup("CanvasGroup");
            _canvasGroup.SetFillSize();
            _canvasGroup.Alpha = 0f;
            _background.AddChild(_canvasGroup);

            SharpUITextMeshPro title = new SharpUITextMeshPro("Title", "GridStrategy\n<size=15%>by Tofunaut</size>");
            title.Font = AppManager.AssetManager.Get<TMPro.TMP_FontAsset>(AssetPaths.Fonts.GravityUltraLight);
            title.Color = Color.white;
            title.TextAlignment = TMPro.TextAlignmentOptions.Center;
            title.SetFillSize(EAxis.X, 0.5f);
            title.SetFixedSize(EAxis.Y, 500);
            title.AutoSizeFont();
            title.alignment = EAlignment.TopCenter;
            _canvasGroup.AddChild(title);

            SharpUITextMeshPro versionLabel = new SharpUITextMeshPro("VersionLabel", AppManager.AppVersion);
            versionLabel.Font = AppManager.AssetManager.Get<TMPro.TMP_FontAsset>(AssetPaths.Fonts.GravityRegular);
            versionLabel.Color = new Color(1f, 1f, 1f, 0.5f);
            versionLabel.TextAlignment = TMPro.TextAlignmentOptions.Center;
            versionLabel.SetFixedSize(100, 50);
            versionLabel.AutoSizeFont();
            versionLabel.alignment = EAlignment.BottomRight;
            versionLabel.margin = new RectOffset(0, 20, 0, 20);
            _canvasGroup.AddChild(versionLabel);

            return _background;
        }

        public override void Show()
        {
            base.Show();

            new TofuAnimation()
                .Value01(BackgroundFadeInTime, EEaseType.Linear, (float newValue) =>
                {
                    _background.Color = Color.Lerp(FadeInStartColor, FadeInEndColor, newValue);
                })
                .Play();

            _canvasGroup.GameObject.SetActive(false);
            new TofuAnimation()
                .Wait(CanvasGroupFadeInDelay)
                .Then()
                .Execute(() =>
                {
                    _canvasGroup.GameObject.SetActive(true);
                })
                .Value01(CanvasGroupFadeInTime, EEaseType.Linear, (float newValue) =>
                {
                    _canvasGroup.Alpha = newValue;
                })
                .Play();
        }
    }
}