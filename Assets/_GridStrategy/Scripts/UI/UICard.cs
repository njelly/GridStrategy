////////////////////////////////////////////////////////////////////////////////
//
//  UICard (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for TofuUnity on 01/14/2020
//
////////////////////////////////////////////////////////////////////////////////

using Tofunaut.SharpUnity.UI;
using UnityEngine;

namespace Tofunaut.GridStrategy.UI
{
    public class UICard : SharpUICanvasGroup
    {
        public static Vector2 Size => new Vector2(630, 880);
        public static Vector2 PrefabViewportSize => new Vector2(580, 400);
        public const int BorderSize = 20;
        public static RectOffset TitleOffset => new RectOffset(30, 30, 30, 30);
        public static Vector2 TitleBarSize => new Vector2(Size.x - BorderSize * 2 - (TitleOffset.left + TitleOffset.right), 50);
        public static Vector2 DescriptionBarSize => new Vector2(TitleBarSize.x, TitleBarSize.y);
        public static Color BorderColor => new Color(0f, 0f, 0f, 1f);
        public static Color BackgroundColor => new Color(0.5f, 0.5f, 0.5f, 1f);
        public static Color TitleColor => new Color(0.8f, 0.8f, 0.8f, 1f);
        public static Color InfoBoxColor => new Color(1f, 1f, 1f, 1f);

        private readonly CardData _cardData;
        private readonly SharpUIImage _cardBorder;
        private readonly SharpUIImage _cardBackground;
        private readonly SharpUIImage _cardTitleBackground;
        private readonly SharpUIImage _prefabViewport; // TODO: make this a rendertexture for a prefab

        public UICard(CardData cardData) : base($"UICard: {cardData.displayName}")
        {
            _cardData = cardData;

            SetFixedSize(Size);

            _cardBorder = new SharpUIImage("CardBorder", null);
            _cardBorder.SetFillSize();
            _cardBorder.Color = BorderColor;
            AddChild(_cardBorder);

            _cardBackground = new SharpUIImage("CardBackground", null);
            _cardBackground.SetFillSize();
            _cardBackground.margin = new RectOffset(BorderSize, BorderSize, BorderSize, BorderSize);
            _cardBackground.Color = BackgroundColor;
            AddChild(_cardBackground);

            _prefabViewport = new SharpUIImage("PrefabViewport", null);
            _prefabViewport.SetFixedSize(PrefabViewportSize);
            _prefabViewport.Color = Color.green;
            _prefabViewport.alignment = EAlignment.TopCenter;
            _prefabViewport.margin = new RectOffset(0, 0, BorderSize + TitleOffset.top + (int)TitleBarSize.y, 0);
            AddChild(_prefabViewport);

            _cardTitleBackground = new SharpUIImage("CardTitleBackground", null);
            _cardTitleBackground.SetFillSize();
            _cardTitleBackground.alignment = EAlignment.TopCenter;
            _cardTitleBackground.margin = TitleOffset;
            AddChild(_cardTitleBackground);
        }
    }
}