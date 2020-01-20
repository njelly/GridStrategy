////////////////////////////////////////////////////////////////////////////////
//
//  DiscardPile (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 01/20/2019
//
////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class DiscardPile
    {
        public IReadOnlyCollection<Card> Cards => _cards.AsReadOnly();

        private readonly List<Card> _cards;
        private readonly Game _game;
        private readonly Player _owner;

        // --------------------------------------------------------------------------------------------
        public DiscardPile(Game game, Player owner)
        {
            _game = game;
            _owner = owner;

            _cards = new List<Card>();
        }

        // --------------------------------------------------------------------------------------------
        public void Add(Card card)
        {
            _cards.Add(card);
        }

        // --------------------------------------------------------------------------------------------
        public Card RemoveTop()
        {
            Card toReturn = _cards[_cards.Count - 1];
            _cards.RemoveAt(_cards.Count - 1);
            return toReturn;
        }

        // --------------------------------------------------------------------------------------------
        public Card Peek()
        {
            return _cards[_cards.Count - 1];
        }
    }
}