////////////////////////////////////////////////////////////////////////////////
//
//  SkillCard (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/29/2019
//
////////////////////////////////////////////////////////////////////////////////


namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class SkillCard : Card
    {
        // --------------------------------------------------------------------------------------------
        public override Type CardType { get { return Type.Skill; } }

        // --------------------------------------------------------------------------------------------
        protected SkillCard(CardData data) : base(data) { }

        // --------------------------------------------------------------------------------------------
        public static SkillCard Create(CardData data)
        {
            return new SkillCard(data);
        }
    }
}