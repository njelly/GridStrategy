////////////////////////////////////////////////////////////////////////////////
//
//  Deck (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for TofuUnity on 01/15/2020
//
////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using Tofunaut.GridStrategy.UI;
using Tofunaut.SharpUnity.UI;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class Deck
    {
        public Player Owner { get; private set; }
        public int NumCardsLeft { get; private set; }

        private readonly Game _game;
        private readonly DeckData _deckData;
        private readonly CardAsDeckMember[] _cards;

        // --------------------------------------------------------------------------------------------
        public Deck(Game game, DeckData deckData, Player owner, uint initialSeed)
        {
            _game = game;
            _deckData = deckData;

            List<CardAsDeckMember> cardsAsList = new List<CardAsDeckMember>();
            foreach (string key in _deckData.cardIdToCount.Keys)
            {
                for(int i = 0; i < _deckData.cardIdToCount[key]; i++)
                {
                    cardsAsList.Add(new CardAsDeckMember
                    {
                        hasBeenDrawn = false,
                        card = new Card(AppManager.Config.GetCardData(key), owner)
                    });
                }
            }
            _cards = cardsAsList.ToArray();

            NumCardsLeft = _cards.Length;

            ShuffleDeck(initialSeed);
        }

        // --------------------------------------------------------------------------------------------
        public void ShuffleDeck(uint seed)
        {
            for(uint i = 0; i < _cards.Length; i++)
            {
                CardAsDeckMember temp = _cards[i];
                int randIndex = _game.Random.Next(seed + i, 0, _cards.Length);
                _cards[i] = _cards[randIndex];
                _cards[randIndex] = temp;
            }
        }

        // --------------------------------------------------------------------------------------------
        public Card DrawNextCard()
        {
            NumCardsLeft = 0;
            Card toReturn = null;

            for(int i = 0; i < _cards.Length; i++)
            {
                if(_cards[i].hasBeenDrawn)
                {
                    continue;
                }
                else
                {
                    NumCardsLeft++;
                }

                if(toReturn == null)
                {
                    CardAsDeckMember cardAsDeckMember = _cards[i];
                    cardAsDeckMember.hasBeenDrawn = true;
                    _cards[i] = cardAsDeckMember;

                    toReturn = _cards[i].card;
                }
            }

            if(toReturn == null)
            {
                Debug.LogError($"There were no cards to draw");
            }
            return toReturn;
        }

        // --------------------------------------------------------------------------------------------
        private struct CardAsDeckMember
        {
            public bool hasBeenDrawn;
            public Card card;
        }
    }
}
