////////////////////////////////////////////////////////////////////////////////
//
//  Deck (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for TofuUnity on 01/15/2020
//
////////////////////////////////////////////////////////////////////////////////

using Tofunaut.GridStrategy.UI;
using Tofunaut.SharpUnity.UI;

namespace Tofunaut.GridStrategy.Game
{
    public class Deck
    {
        public Player Owner { get; private set; }

        private readonly DeckData _deckData;

        public Deck(DeckData deckData, Player owner)
        {
            _deckData = deckData;
        }
    }
}
