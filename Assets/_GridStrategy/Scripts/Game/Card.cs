////////////////////////////////////////////////////////////////////////////////
//
//  Card (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/29/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class Card
    {
        public Player Owner { get { return _owner; } }

        public readonly string name;
        public readonly int solidarityRequired;

        private Player _owner;

        // --------------------------------------------------------------------------------------------
        public Card(CardData data, Player owner)
        {
            name = data.id;
            _owner = owner;
            solidarityRequired = data.energyRequired;
        }

        // --------------------------------------------------------------------------------------------
        public class CardEventArgs : EventArgs
        {
            public readonly Card card;

            public CardEventArgs(Card card)
            {
                this.card = card;
            }
        }
    }
}