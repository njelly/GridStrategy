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

        private readonly CardData _cardData;

        public UICard(CardData cardData) : base($"UICard: {cardData.displayName}")
        {
            _cardData = cardData;
        }
    }
}