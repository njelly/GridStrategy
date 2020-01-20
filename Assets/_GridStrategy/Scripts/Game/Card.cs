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
        private static uint _idCounter;

        public Player Owner { get { return _owner; } }

        public readonly CardData cardData;
        public readonly string name;
        public readonly int solidarityRequired;

        private readonly uint _id;

        private Player _owner;

        // --------------------------------------------------------------------------------------------
        public Card(CardData data, Player owner)
        {
            cardData = data;
            name = data.id;
            _owner = owner;
            solidarityRequired = data.energyRequired;

            _id = _idCounter++;
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