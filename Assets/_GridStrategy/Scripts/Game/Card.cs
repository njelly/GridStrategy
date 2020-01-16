////////////////////////////////////////////////////////////////////////////////
//
//  Card (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/29/2019
//
////////////////////////////////////////////////////////////////////////////////

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
        protected Card(CardData data)
        {
            name = data.id;
            solidarityRequired = data.energyRequired;
        }

        // --------------------------------------------------------------------------------------------
        public void Discard()
        {
            _owner.Discard(this);
        }

        // --------------------------------------------------------------------------------------------
        public static Card Create(Player owner, CardData data)
        {
            Card toReturn = new Card(data);

            toReturn._owner = owner;

            return toReturn;
        }
    }
}