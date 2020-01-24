////////////////////////////////////////////////////////////////////////////////
//
//  UICard (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for TofuUnity on 01/14/2020
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Tofunaut.SharpUnity;
using Tofunaut.SharpUnity.UI;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Tofunaut.GridStrategy.UI
{
    // --------------------------------------------------------------------------------------------
    public class UICard : SharpUICanvasGroup
    {
        public const int BorderSize = 10;
        public static Vector2 Size => new Vector2(316, 440);
        public static Vector2 CardBackgroundSize = new Vector2(Size.x - (BorderSize * 2), Size.y - (BorderSize * 2));
        public static Vector2 ViewportSize => new Vector2(CardBackgroundSize.x - (BorderSize * 2), 180);
        public static RectOffset TitleOffset => new RectOffset(BorderSize, BorderSize, BorderSize, BorderSize);
        public static Vector2 TitleBarSize => new Vector2(Size.x - BorderSize * 2 - (TitleOffset.left + TitleOffset.right), 24);
        public static Vector2 InfoBarSize => new Vector2(TitleBarSize.x, TitleBarSize.y);
        public static RectOffset InfoBarOffset => new RectOffset(0, 0, (int)(TitleOffset.top + TitleBarSize.y + ViewportSize.y), 0);
        public static Vector2 DescriptionSize = new Vector2(Size.x - (BorderSize * 4), Size.y - ViewportSize.y - TitleBarSize.y - InfoBarSize.y - (BorderSize * 4));
        public static RectOffset DescriptionOffset => new RectOffset(0, 0, (int)(InfoBarOffset.top + InfoBarSize.y), 0);
        public static Color BorderColor => new Color(0f, 0f, 0f, 1f);
        public static Color BackgroundColor => new Color(0.5f, 0.5f, 0.5f, 1f);
        public static Color TitleColor => new Color(0.8f, 0.8f, 0.8f, 1f);
        public static Color DescriptionBoxColor => new Color(1f, 1f, 1f, 1f);

        public CardData CardData => _cardData;

        private readonly CardData _cardData;
        private readonly SharpUIImage _border;
        private readonly SharpUIImage _cardBackground;
        private readonly SharpUIImage _titleBackground;
        private readonly SharpUIImage _prefabViewportBackground;
        private readonly SharpUIPrefabToRenderTexture _prefabViewport;
        private readonly SharpUIImage _infoBarBackground;
        private readonly SharpUIImage _descriptionBackground;

        // --------------------------------------------------------------------------------------------
        public UICard(CardData cardData) : base($"UICard: {cardData.id}")
        {
            _cardData = cardData;

            SetFixedSize(Size);

            _border = new SharpUIImage("CardBorder", null);
            _border.SetFillSize();
            _border.Color = BorderColor;
            AddChild(_border);

            _cardBackground = new SharpUIImage("CardBackground", null);
            _cardBackground.SetFixedSize(CardBackgroundSize);
            _cardBackground.margin = new RectOffset(BorderSize, BorderSize, BorderSize, BorderSize);
            _cardBackground.Color = BackgroundColor;
            AddChild(_cardBackground);

            _prefabViewportBackground = new SharpUIImage("PrefabViewportBackground", null);
            _prefabViewportBackground.Color = Color.green; // placeholder
            _prefabViewportBackground.SetFixedSize(ViewportSize);
            _prefabViewportBackground.margin = new RectOffset(BorderSize, 0, BorderSize + (int)TitleBarSize.y, 0);
            _cardBackground.AddChild(_prefabViewportBackground);

            if(!string.IsNullOrEmpty(cardData.illustrationPrefabPath))
            {
                _prefabViewport = new SharpUIPrefabToRenderTexture("PrefabViewport", AppManager.AssetManager.Get<GameObject>(cardData.illustrationPrefabPath), ViewportSize, true);
                _prefabViewport.SetFillSize();
                _prefabViewport.SetCameraDistanceAndAngle(new Vector3(0f, 0.5f, 0f), 2f, Quaternion.Euler(0f, 30f, 10f));
                _prefabViewportBackground.AddChild(_prefabViewport);
            }

            _descriptionBackground = new SharpUIImage("DescriptionBackground", null);
            _descriptionBackground.SetFixedSize(DescriptionSize);
            _descriptionBackground.margin = DescriptionOffset;
            _descriptionBackground.Color = DescriptionBoxColor;
            _descriptionBackground.alignment = EAlignment.TopCenter;
            _cardBackground.AddChild(_descriptionBackground);

            _titleBackground = new SharpUIImage("TitleBackground", null);
            _titleBackground.Color = TitleColor;
            _titleBackground.SetFixedSize(TitleBarSize);
            _titleBackground.alignment = EAlignment.TopCenter;
            _titleBackground.margin = TitleOffset;
            _cardBackground.AddChild(_titleBackground);

            SharpUITextMeshPro titleBackgroundLabel = new SharpUITextMeshPro("TitleLabel", cardData.id);
            titleBackgroundLabel.SetFillSize();
            titleBackgroundLabel.AutoSizeFont();
            titleBackgroundLabel.TextAlignment = TMPro.TextAlignmentOptions.Left;
            titleBackgroundLabel.Font = AppManager.AssetManager.Get<TMPro.TMP_FontAsset>(AssetPaths.Fonts.GravityRegular);
            titleBackgroundLabel.alignment = EAlignment.MiddleLeft;
            titleBackgroundLabel.margin = new RectOffset(20, 0, 0, 0);
            titleBackgroundLabel.Color = Color.black;
            _titleBackground.AddChild(titleBackgroundLabel);

            _infoBarBackground = new SharpUIImage("InfoBarBackground", null);
            _infoBarBackground.Color = TitleColor;
            _infoBarBackground.SetFixedSize(TitleBarSize);
            _infoBarBackground.alignment = EAlignment.TopCenter;
            _infoBarBackground.margin = InfoBarOffset;
            _cardBackground.AddChild(_infoBarBackground);
        }

        // --------------------------------------------------------------------------------------------
        public static void LoadRequiredAssets(CardData cardData, Action onComplete)
        {
            int numLoadCalls = 0;
            int numCompletedLoadCalls = 0;

            void loadCompleteCallback()
            {
                numCompletedLoadCalls++;
                if (numCompletedLoadCalls >= numLoadCalls)
                {
                    onComplete?.Invoke();
                }
            }

            if (!string.IsNullOrEmpty(cardData.illustrationPrefabPath))
            {
                numLoadCalls++;
                AppManager.AssetManager.Load(cardData.illustrationPrefabPath, (bool succesfull, GameObject payload) =>
                {
                    if(succesfull)
                    {
                        loadCompleteCallback();
                    }
                });
            }

            if(numLoadCalls == 0)
            {
                onComplete?.Invoke();
            }
        }

        // --------------------------------------------------------------------------------------------
        public static void ReleaseRequiredAssets(CardData cardData)
        {
            if (!string.IsNullOrEmpty(cardData.illustrationPrefabPath))
            {
                AppManager.AssetManager.Release<GameObject>(cardData.illustrationPrefabPath);
            }
        }
    }
}