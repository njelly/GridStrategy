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
    public abstract class Card
    {
        public enum Type
        {
            Invalid = 0,
            Ally = 1,
            Skill = 2,
            Equip = 3,
        }

        // --------------------------------------------------------------------------------------------
        public abstract Type CardType { get; }

        public Player Owner { get { return _owner; } }

        public readonly string name;
        public readonly int solidarityRequired;

        private Player _owner;

        // --------------------------------------------------------------------------------------------
        protected Card(CardData data)
        {
            name = data.displayName;
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
            Card toReturn;
            switch (data.type)
            {
                case Type.Invalid:
                    Debug.LogError("can't create a card with an invalid type");
                    return null;
                case Type.Ally:
                    toReturn = AllyCard.Create(data);
                    break;
                case Type.Skill:
                    toReturn = SkillCard.Create(data);
                    break;
                case Type.Equip:
                    toReturn = EquipCard.Create(data);
                    break;
                default:
                    Debug.LogError($"the type {data.type} has not been implemented.");
                    return null;
            }

            toReturn._owner = owner;

            return toReturn;
        }
    }
}