////////////////////////////////////////////////////////////////////////////////
//
//  UIStartMenuView (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/09/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using Tofunaut.Animation;
using Tofunaut.SharpUnity.UI;
using Tofunaut.SharpUnity.UI.Behaviour;
using UnityEngine;

namespace Tofunaut.GridStrategy.UI
{

    // --------------------------------------------------------------------------------------------
    public class UIStartMenuView : SharpUIView
    {
        public interface IUIStartMenuViewListener
        {
            void OnPlayClicked();
            void OnLoadoutClicked();
            void OnSettingsClicked();
        }

        private const float BackgroundFadeInTime = 6f;
        private static readonly Color FadeInStartColor = new Color(0f, 0f, 0f, 1f);
        private static readonly Color FadeInEndColor = new Color(0f, 0f, 0f, 0f);

        private const float CanvasGroupFadeInDelay = 1f;
        private const float CanvasGroupFadeInTime = 3f;

        private IUIStartMenuViewListener _listener;
        private SharpUIImage _background;
        private SharpUICanvasGroup _canvasGroup;
        private SharpUIBase _buttonLayout;


        // --------------------------------------------------------------------------------------------
        public UIStartMenuView(IUIStartMenuViewListener listener) : base(UIMainCanvas.Instance) 
        {
            _listener = listener;
        }


        // --------------------------------------------------------------------------------------------
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

            _buttonLayout = BuildButtonLayout();
            _canvasGroup.AddChild(_buttonLayout);

            return _background;
        }


        // --------------------------------------------------------------------------------------------
        private SharpUIBase BuildButtonLayout()
        {
            SharpUIVerticalLayout toReturn = new SharpUIVerticalLayout("ButtonLayout");
            toReturn.alignment = EAlignment.MiddleCenter;
            toReturn.childAlignment = EAlignment.MiddleCenter;
            toReturn.order = EVerticalOrder.BottomToTop;
            toReturn.spacing = 20;
            toReturn.SetFitSize();

            toReturn.AddChild(new StartMenuButton("Play", _listener.OnPlayClicked));
            toReturn.AddChild(new StartMenuButton("Loadout", _listener.OnLoadoutClicked));
            toReturn.AddChild(new StartMenuButton("Settings", _listener.OnSettingsClicked));

            return toReturn;
        }


        // --------------------------------------------------------------------------------------------
        public override void Show()
        {
            base.Show();


            // HACK TO TEST VISUALS, FIX LAYOUT
            _buttonLayout.RectTransform.anchoredPosition = new Vector2(1150, 320);

            // fade in the background (by fading out a background image from black to clear)
            new TofuAnimation()
                .Value01(BackgroundFadeInTime, EEaseType.Linear, (float newValue) =>
                {
                    _background.Color = Color.Lerp(FadeInStartColor, FadeInEndColor, newValue);
                })
                .Play();

            // fade in the canvas group
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


        private class StartMenuButton : SharpUIBase
        {
            private static readonly Vector2 Size = new Vector2(300, 60);
            private const float HoverAnimTime = 0.5f;
            private Color DefaultColor = new Color(1f, 1f, 1f, 0.5f);
            private Color HighlightedColor = new Color(0f, 0f, 0f, 1f);
            private Color DefaultBackgroundColor = new Color(1f, 1f, 1f, 0f);
            private Color HighlightedBackgroundColor = new Color(1f, 1f, 1f, 0.5f);

            private SharpUIImage _background;
            private SharpUITextMeshPro _captionLabel;
            private TofuAnimation _pointerEnterAnim;
            private TofuAnimation _pointerExitAnim;
            private TofuAnimation _pointerDownAnim;
            private TofuAnimation _pointerUpAnim;

            public StartMenuButton(string caption, Action onClicked) : base($"{caption}_Button")
            {
                SetFixedSize(Size);
                alignment = EAlignment.MiddleCenter;

                _background = new SharpUIImage($"{name}_Background", null);
                _background.SetFillSize(EAxis.X);
                _background.SetFixedSize(EAxis.Y, 0);
                _background.alignment = EAlignment.MiddleCenter;
                _background.Color = DefaultBackgroundColor;
                AddChild(_background);

                _captionLabel = new SharpUITextMeshPro($"{caption}_Label", caption);
                _captionLabel.Font = AppManager.AssetManager.Get<TMPro.TMP_FontAsset>(AssetPaths.Fonts.GravityBook);
                _captionLabel.Color = DefaultColor;
                _captionLabel.TextAlignment = TMPro.TextAlignmentOptions.Center;
                _captionLabel.SetFillSize();
                _captionLabel.AutoSizeFont();
                _captionLabel.alignment = EAlignment.TopCenter;
                AddChild(_captionLabel);

                SubscribeToEvent(EEventType.PointerEnter, (object sender, EventSystemEventArgs e) =>
                {
                    _pointerEnterAnim?.Stop();
                    _pointerExitAnim?.Stop();
                    _pointerDownAnim?.Stop();
                    _pointerUpAnim?.Stop();

                    Vector2 startSize = new Vector2(_background.RectTransform.sizeDelta.x, 0f);
                    Vector2 endSize = Size;
                    float startLerp = Mathf.InverseLerp(startSize.y, endSize.y, _background.RectTransform.sizeDelta.y);
                    _pointerEnterAnim = new TofuAnimation()
                        .ValueFromTo(startLerp, 1f, Mathf.Lerp(0f, HoverAnimTime, 1f - startLerp), EEaseType.EaseOutExpo, (float newValue) =>
                        {
                            _background.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.LerpUnclamped(startSize.y, endSize.y, newValue));
                            _background.Color = Color.Lerp(DefaultBackgroundColor, HighlightedBackgroundColor, newValue);
                            _captionLabel.Color = Color.Lerp(DefaultColor, HighlightedColor, newValue);
                        })
                        .Then()
                        .Execute(() =>
                        {
                            _pointerEnterAnim = null;
                        })
                        .Play();
                });

                SubscribeToEvent(EEventType.PointerExit, (object sender, EventSystemEventArgs e) =>
                {
                    _pointerEnterAnim?.Stop();
                    _pointerExitAnim?.Stop();
                    _pointerDownAnim?.Stop();
                    _pointerUpAnim?.Stop();

                    Vector2 startSize = new Vector2(_background.RectTransform.sizeDelta.x, 0f);
                    Vector2 endSize = Size;
                    float startLerp = Mathf.InverseLerp(startSize.y, endSize.y, _background.RectTransform.sizeDelta.y);
                    _pointerExitAnim = new TofuAnimation()
                        .ValueFromTo(startLerp, 0f, 1f - Mathf.Lerp(0f, HoverAnimTime, startLerp), EEaseType.EaseOutExpo, (float newValue) =>
                        {
                            _background.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.LerpUnclamped(startSize.y, endSize.y, newValue));
                            _background.Color = Color.Lerp(DefaultBackgroundColor, HighlightedBackgroundColor, newValue);
                            _captionLabel.Color = Color.Lerp(DefaultColor, HighlightedColor, newValue);
                        })
                        .Then()
                        .Execute(() =>
                        {
                            _pointerExitAnim = null;
                        })
                        .Play();
                });
            }
        }
    }
}