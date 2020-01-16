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

namespace Tofunaut.GridStrategy.UI
{
    // --------------------------------------------------------------------------------------------
    public class UICard : SharpUICanvasGroup
    {
        public static Vector2 Size => new Vector2(630, 880);
        public const int BorderSize = 20;
        public static Vector2 ViewportSize => new Vector2(Size.x - (BorderSize * 2), 400);
        public static RectOffset TitleOffset => new RectOffset(30, 30, 30, 30);
        public static Vector2 TitleBarSize => new Vector2(Size.x - BorderSize * 2 - (TitleOffset.left + TitleOffset.right), 50);
        public static Vector2 InfoBarSize => new Vector2(TitleBarSize.x, TitleBarSize.y);
        public static RectOffset InfoBarOffset => new RectOffset(0, 0, (int)(BorderSize + TitleOffset.top + TitleBarSize.y + ViewportSize.y), 0);
        public static Vector2 DescriptionSize = new Vector2(Size.x - (BorderSize * 2), Size.y - ViewportSize.y - TitleBarSize.y - InfoBarSize.y);
        public static RectOffset DescriptionOffset => new RectOffset(0, 0, (int)(InfoBarOffset.top + InfoBarSize.y), 0);
        public static Color BorderColor => new Color(0f, 0f, 0f, 1f);
        public static Color BackgroundColor => new Color(0.5f, 0.5f, 0.5f, 1f);
        public static Color TitleColor => new Color(0.8f, 0.8f, 0.8f, 1f);
        public static Color DescriptionBoxColor => new Color(1f, 1f, 1f, 1f);

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
            _cardBackground.SetFillSize();
            _cardBackground.margin = new RectOffset(BorderSize, BorderSize, BorderSize, BorderSize);
            _cardBackground.Color = BackgroundColor;
            AddChild(_cardBackground);

            _prefabViewportBackground = new SharpUIImage("PrefabViewportBackground", null);
            _prefabViewportBackground.Color = Color.green; // placeholder
            _prefabViewportBackground.SetFixedSize(ViewportSize);
            _prefabViewportBackground.alignment = EAlignment.TopCenter;
            _prefabViewportBackground.margin = new RectOffset(0, 0, BorderSize + TitleOffset.top + (int)TitleBarSize.y, 0);
            AddChild(_prefabViewportBackground);

            if(!string.IsNullOrEmpty(cardData.illustrationPrefabPath))
            {
                Debug.Log(cardData.illustrationPrefabPath);
                _prefabViewport = new SharpUIPrefabToRenderTexture("PrefabViewport", AppManager.AssetManager.Get<GameObject>(cardData.illustrationPrefabPath), ViewportSize);
                _prefabViewport.SetFillSize();
                _prefabViewport.SetCameraDistanceAndAngle(Vector3.zero, 5f, Quaternion.Euler(0f, 0f, 30f));
                _prefabViewportBackground.AddChild(_prefabViewport);
            }

            _descriptionBackground = new SharpUIImage("DescriptionBackground", null);
            _descriptionBackground.SetFixedSize(DescriptionSize);
            _descriptionBackground.margin = DescriptionOffset;
            _descriptionBackground.Color = DescriptionBoxColor;
            _descriptionBackground.alignment = EAlignment.TopCenter;
            AddChild(_descriptionBackground);

            _titleBackground = new SharpUIImage("TitleBackground", null);
            _titleBackground.Color = TitleColor;
            _titleBackground.SetFixedSize(TitleBarSize);
            _titleBackground.alignment = EAlignment.TopCenter;
            _titleBackground.margin = TitleOffset;
            AddChild(_titleBackground);

            _infoBarBackground = new SharpUIImage("InfoBarBackground", null);
            _infoBarBackground.Color = TitleColor;
            _infoBarBackground.SetFixedSize(TitleBarSize);
            _infoBarBackground.alignment = EAlignment.TopCenter;
            _infoBarBackground.margin = InfoBarOffset;
            AddChild(_infoBarBackground);
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