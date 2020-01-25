////////////////////////////////////////////////////////////////////////////////
//
//  Hand (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 01/20/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class Hand
    {
        public event EventHandler<Card.CardEventArgs> OnPlayerDrewCard;
        public event EventHandler<Card.CardEventArgs> OnPlayerDiscardedCard;

        public IReadOnlyCollection<Card> Cards => _cards.AsReadOnly();

        private readonly Game _game;
        private readonly Player _owner;
        private readonly Deck _deck;
        private readonly List<Card> _cards;
        private readonly DiscardPile _discardPile;

        // --------------------------------------------------------------------------------------------
        public Hand(Game game, Player owner, Deck deck, DiscardPile discardPile)
        {
            _game = game;
            _owner = owner;
            _deck = deck;
            _discardPile = discardPile;

            _cards = new List<Card>();
        }

        // --------------------------------------------------------------------------------------------
        public void DrawCard()
        {
            Card drawnCard = _deck.DrawNextCard();
            _cards.Add(drawnCard);

            OnPlayerDrewCard?.Invoke(this, new Card.CardEventArgs(drawnCard));
        }

        // --------------------------------------------------------------------------------------------
        public void DiscardCard(Card card)
        {
            if(!_cards.Remove(card))
            {
                Debug.LogError($"the card {card.name} is not in player {_owner.playerIndex}'s hand");
                return;
            }

            OnPlayerDiscardedCard?.Invoke(this, new Card.CardEventArgs(card));
        }

        // --------------------------------------------------------------------------------------------
        public bool ContainsCard(Card card)
        {
            return _cards.Contains(card);
        }
    }
}