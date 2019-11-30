////////////////////////////////////////////////////////////////////////////////
//
//  EquipCard (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/29/2019
//
////////////////////////////////////////////////////////////////////////////////


namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class EquipCard : Card
    {
        // --------------------------------------------------------------------------------------------
        public override Type CardType { get { return Type.Skill; } }

        // --------------------------------------------------------------------------------------------
        protected EquipCard(CardData data) : base(data) { }

        // --------------------------------------------------------------------------------------------
        public static EquipCard Create(CardData data)
        {
            return new EquipCard(data);
        }
    }
}