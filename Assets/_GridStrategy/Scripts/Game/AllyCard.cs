////////////////////////////////////////////////////////////////////////////////
//
//  AllyCard (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/29/2019
//
////////////////////////////////////////////////////////////////////////////////

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    /// <summary>
    /// An AllyCard spawns a new Unit on the board when played.
    /// </summary>
    public class AllyCard : Card
    {
        // --------------------------------------------------------------------------------------------
        public override Type CardType { get { return Type.Ally; } }

        // --------------------------------------------------------------------------------------------
        protected AllyCard(CardData data) : base(data) { }

        // --------------------------------------------------------------------------------------------
        public static AllyCard Create(CardData data)
        {
            return new AllyCard(data);
        }
    }
}