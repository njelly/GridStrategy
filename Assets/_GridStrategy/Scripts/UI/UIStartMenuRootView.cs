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
    public class UIStartMenuRootView : SharpUIView
    {
        public interface IUIStartMenuRootViewListener
        {
            void OnRootNewGameClicked();
            void OnRootContinueClicked();
            void OnRootMultiplayerClicked();
            void OnRootOptionsClicked();
        }

        private const float BackgroundFadeInTime = 6f;
        private static readonly Color FadeInStartColor = new Color(0f, 0f, 0f, 1f);
        private static readonly Color FadeInEndColor = new Color(0f, 0f, 0f, 0f);

        private const float CanvasGroupFadeInDelay = 1f;
        private const float CanvasGroupFadeInTime = 3f;

        private IUIStartMenuRootViewListener _listener;
        private SharpUIImage _background;
        private SharpUICanvasGroup _canvasGroup;
        private SharpUIBase _buttonLayout;
        private UIStartMenuButton _newGameButton;
        private UIStartMenuButton _continueButton;
        private UIStartMenuButton _multiplayerButton;
        private UIStartMenuButton _optionsButton;


        // --------------------------------------------------------------------------------------------
        public UIStartMenuRootView(IUIStartMenuRootViewListener listener) : base(UIMainCanvas.Instance)
        {
            _listener = listener;
        }


        // --------------------------------------------------------------------------------------------
        protected override SharpUIBase BuildMainPanel()
        {
            _background = new SharpUIImage("UIStartMenuView", null);
            _background.Color = FadeInStartColor;
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
            versionLabel.SetFixedSize(50, 25);
            versionLabel.AutoSizeFont();
            versionLabel.alignment = EAlignment.BottomRight;
            versionLabel.margin = new RectOffset(0, 20, 0, 20);
            _canvasGroup.AddChild(versionLabel);

            _buttonLayout = BuildButtonLayout();
            _canvasGroup.AddChild(_buttonLayout);

            if(!AppManager.IsClientValid)
            {
                SharpUITextMeshPro clientValidationErrorMessage = new SharpUITextMeshPro("ClientValidationErrorMessage", string.Empty);
                clientValidationErrorMessage.Font = AppManager.AssetManager.Get<TMPro.TMP_FontAsset>(AssetPaths.Fonts.GravityRegular);
                clientValidationErrorMessage.Color = Color.red;
                clientValidationErrorMessage.SetFixedSize(200, 25);
                clientValidationErrorMessage.SetFontSize(28);
                clientValidationErrorMessage.alignment = EAlignment.TopLeft;
                clientValidationErrorMessage.margin = new RectOffset(20, 0, 20, 0);
                clientValidationErrorMessage.TextAlignment = TMPro.TextAlignmentOptions.Left;
                _canvasGroup.AddChild(clientValidationErrorMessage);

                switch(AppManager.ClientState)
                {
                    case AppManager.EClientState.ValidationError:
                        clientValidationErrorMessage.Text = "Client failed to validate correctly";
                        break;
                    case AppManager.EClientState.Offline:
                        clientValidationErrorMessage.Text = "Client is offline";
                        break;
                    case AppManager.EClientState.NeedsUpdate:
                        clientValidationErrorMessage.Text = "Client must update";
                        break;
                }
            }

            return _background;
        }


        // --------------------------------------------------------------------------------------------
        private SharpUIBase BuildButtonLayout()
        {
            SharpUIVerticalLayout toReturn = new SharpUIVerticalLayout("ButtonLayout");
            toReturn.alignment = EAlignment.BottomCenter;
            toReturn.margin = new RectOffset(0, 0, 0, 200);
            toReturn.childAlignment = EAlignment.MiddleCenter;
            toReturn.spacing = 50;
            toReturn.SetFitSize();

            _newGameButton = new UIStartMenuButton("New Game", () => { AnimateNewGameButtonSelected(_listener.OnRootNewGameClicked); });
            toReturn.AddChild(_newGameButton);

            _continueButton = new UIStartMenuButton("Continue", () => { AnimateContinueButtonSelected(_listener.OnRootContinueClicked); });
            toReturn.AddChild(_continueButton);

            _multiplayerButton = new UIStartMenuButton("Multiplayer", () => { AnimateMultiplayerButtonSelected(_listener.OnRootMultiplayerClicked); });
            toReturn.AddChild(_multiplayerButton);

            _optionsButton = new UIStartMenuButton("Options", () => { AnimateOptionsButtonSelected(_listener.OnRootOptionsClicked); });
            toReturn.AddChild(_optionsButton);

            return toReturn;
        }


        // --------------------------------------------------------------------------------------------
        public override void Show()
        {
            base.Show();

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


        // --------------------------------------------------------------------------------------------
        // TODO: put these into one function somehow
        private void AnimateNewGameButtonSelected(Action onComplete)
        {
            _newGameButton.AnimateSelected(onComplete);
            _continueButton.AnimateAway(0.3f);
            _multiplayerButton.AnimateAway(0.2f);
            _optionsButton.AnimateAway(0.1f);
        }
        private void AnimateContinueButtonSelected(Action onComplete)
        {
            _newGameButton.AnimateAway(0.3f);
            _continueButton.AnimateSelected(onComplete);
            _multiplayerButton.AnimateAway(0.2f);
            _optionsButton.AnimateAway(0.1f);
        }
        private void AnimateMultiplayerButtonSelected(Action onComplete)
        {
            _newGameButton.AnimateAway(0.3f);
            _continueButton.AnimateAway(0.2f);
            _multiplayerButton.AnimateSelected(onComplete);
            _optionsButton.AnimateAway(0.1f);
        }
        private void AnimateOptionsButtonSelected(Action onComplete)
        {
            _newGameButton.AnimateAway(0.3f);
            _continueButton.AnimateAway(0.2f);
            _multiplayerButton.AnimateAway(0.1f);
            _optionsButton.AnimateSelected(onComplete);
        }


        // --------------------------------------------------------------------------------------------
        private class UIStartMenuButton : SharpUIBase
        {
            private static readonly Vector2 Size = new Vector2(300, 60);
            private const float HoverAnimTime = 0.5f;
            private static readonly Color DefaultColor = new Color(1f, 1f, 1f, 0.5f);
            private static readonly Color HighlightedColor = new Color(0f, 0f, 0f, 1f);
            private static readonly Color DefaultBackgroundColor = new Color(1f, 1f, 1f, 0f);
            private static readonly Color HighlightedBackgroundColor = new Color(1f, 1f, 1f, 0.5f);
            private const float PressedScale = 0.9f;
            private const float PressAnimTime = 0.3f;
            private const float AnimateAwayDistance = -300;
            private const float AnimateAwayTime = 0.5f;
            private const float AnimateSelectedDistance = 50;
            private const float AnimateSelectedTime = 0.5f;
            private const float AnimateSelectedScale = 1.2f;

            private Action _onClicked;
            private SharpUIImage _background;
            private SharpUITextMeshPro _captionLabel;
            private TofuAnimation _pointerEnterAnim;
            private TofuAnimation _pointerExitAnim;
            private TofuAnimation _pointerDownAnim;
            private TofuAnimation _animateAwayAnim;
            private TofuAnimation _animateSelectedAnim;

            // --------------------------------------------------------------------------------------------
            public UIStartMenuButton(string caption, Action onClicked) : base($"{caption}_Button")
            {
                SetFixedSize(Size);
                alignment = EAlignment.MiddleCenter;

                _onClicked = onClicked;

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
                _captionLabel.alignment = EAlignment.MiddleCenter;
                AddChild(_captionLabel);

                SubscribeToEvent(EEventType.PointerEnter, OnPointerEnter);
                SubscribeToEvent(EEventType.PointerExit, OnPointerExit);
                SubscribeToEvent(EEventType.PointerDown, OnPointerDown);
                SubscribeToEvent(EEventType.PointerUp, OnPointerUp);
                SubscribeToEvent(EEventType.PointerClick, OnClicked);
            }

            public override void Destroy()
            {
                base.Destroy();

                _pointerEnterAnim?.Stop();
                _pointerExitAnim?.Stop();
                _pointerDownAnim?.Stop();
                _animateAwayAnim?.Stop();
                _animateSelectedAnim?.Stop();
            }

            // --------------------------------------------------------------------------------------------
            private void OnPointerEnter(object sender, EventSystemEventArgs e )
            {
                Highlight();
            }


            // --------------------------------------------------------------------------------------------
            private void OnPointerExit(object sender, EventSystemEventArgs e)
            {
                UnHighlight();
            }


            // --------------------------------------------------------------------------------------------
            private void OnPointerDown(object sender, EventSystemEventArgs e)
            {
                Press();
            }


            // --------------------------------------------------------------------------------------------
            private void OnPointerUp(object sender, EventSystemEventArgs e)
            {
                UnPress();
            }


            // --------------------------------------------------------------------------------------------
            private void OnClicked(object sender, EventSystemEventArgs e)
            {
                _onClicked?.Invoke();
            }


            // --------------------------------------------------------------------------------------------
            public void Highlight()
            {
                _pointerEnterAnim?.Stop();
                _pointerExitAnim?.Stop();

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
            }


            // --------------------------------------------------------------------------------------------
            public void UnHighlight()
            {
                _pointerEnterAnim?.Stop();
                _pointerExitAnim?.Stop();

                // reset pointer down anim
                _pointerDownAnim?.Stop();
                _captionLabel.Transform.localScale = Vector3.one;

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
            }


            // --------------------------------------------------------------------------------------------
            public void Press()
            {
                _pointerDownAnim?.Stop();

                Vector3 startScale = Vector3.one;
                Vector3 endScale = Vector3.one * PressedScale;
                _pointerDownAnim = new TofuAnimation()
                    .Value01(PressAnimTime, EEaseType.EaseOutExpo, (float newValue) =>
                    {
                        _captionLabel.Transform.localScale = Vector3.LerpUnclamped(startScale, endScale, newValue);
                    })
                    .Then()
                    .Execute(() =>
                    {
                        _pointerDownAnim = null;
                    })
                    .Play();
            }


            // --------------------------------------------------------------------------------------------
            public void UnPress()
            {
                _pointerDownAnim?.Stop();
                _captionLabel.Transform.localScale = Vector3.one;
            }


            // --------------------------------------------------------------------------------------------
            public void AnimateAway(float delay)
            {
                UnsubscribeToEvent(EEventType.PointerEnter, OnPointerEnter);
                UnsubscribeToEvent(EEventType.PointerExit, OnPointerExit);
                UnsubscribeToEvent(EEventType.PointerDown, OnPointerDown);
                UnsubscribeToEvent(EEventType.PointerUp, OnPointerUp);
                UnsubscribeToEvent(EEventType.PointerClick, OnClicked);

                Vector2 startPosition = RectTransform.anchoredPosition;
                Vector2 endPosition = startPosition + Vector2.up * AnimateAwayDistance;
                Color startColor = _captionLabel.Color;
                Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
                _animateAwayAnim = new TofuAnimation()
                    .Wait(delay)
                    .Then()
                    .Value01(AnimateAwayTime, EEaseType.EaseOutExpo, (float newValue) =>
                    {
                        RectTransform.anchoredPosition = Vector2.LerpUnclamped(startPosition, endPosition, newValue);
                        _captionLabel.Color = Color.Lerp(startColor, endColor, newValue);
                    })
                    .Play();
            }


            // --------------------------------------------------------------------------------------------
            public void AnimateSelected(Action onComplete)
            {
                UnHighlight();

                UnsubscribeToEvent(EEventType.PointerEnter, OnPointerEnter);
                UnsubscribeToEvent(EEventType.PointerExit, OnPointerExit);
                UnsubscribeToEvent(EEventType.PointerDown, OnPointerDown);
                UnsubscribeToEvent(EEventType.PointerUp, OnPointerUp);
                UnsubscribeToEvent(EEventType.PointerClick, OnClicked);

                Vector2 startPosition = RectTransform.anchoredPosition;
                Vector2 endPosition = startPosition  + Vector2.up * AnimateSelectedTime;
                Color startColor = _captionLabel.Color;
                Color endColor = Color.white;
                Vector3 startScale = Vector3.one;
                Vector3 endScale = Vector3.one * AnimateSelectedScale;
                _animateSelectedAnim = new TofuAnimation()
                    .Value01(AnimateAwayTime, EEaseType.EaseOutExpo, (float newValue) =>
                    {
                        RectTransform.anchoredPosition = Vector2.LerpUnclamped(startPosition, endPosition, newValue);
                        _captionLabel.Color = Color.Lerp(startColor, endColor, newValue);
                        RectTransform.localScale = Vector3.Lerp(startScale, endScale, newValue);
                    })
                    .Then()
                    .Execute(() =>
                    {
                        onComplete?.Invoke();
                    })
                    .Play();
            }
        }
    }
}
