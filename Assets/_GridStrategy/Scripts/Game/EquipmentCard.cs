////////////////////////////////////////////////////////////////////////////////
//
//  EquipmentCard (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/29/2019
//
////////////////////////////////////////////////////////////////////////////////


namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class EquipmentCard : Card
    {
        // --------------------------------------------------------------------------------------------
        public override Type CardType { get { return Type.Skill; } }

        // --------------------------------------------------------------------------------------------
        protected EquipmentCard(CardData data) : base(data) { }

        // --------------------------------------------------------------------------------------------
        public static EquipmentCard Create(CardData data)
        {
            return new EquipmentCard(data);
        }
    }
}