////////////////////////////////////////////////////////////////////////////////
//
//  Player (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/29/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class Player
    {
        public static event EventHandler<PlayerEventArgs> PlayerTurnStarted;
        public static event EventHandler<PlayerEventArgs> PlayerTurnEnded;
        public event EventHandler<PlayerEventArgs> PlayerLost;

        public IReadOnlyCollection<Card> Deck { get { return _deck.AsReadOnly(); } }
        public IReadOnlyCollection<Card> Hand { get { return _hand.AsReadOnly(); } }
        public IReadOnlyCollection<Card> DiscardPile { get { return _discardPile.AsReadOnly(); } }
        public IReadOnlyCollection<Unit> Units { get { return _units.AsReadOnly(); } }
        public Unit Hero { get { return _hero; } }
        public PlayerData PlayerData { get { return _playerData; } }
        public bool HasLost => Hero.IsDead;

        /// <summary>
        /// The current energy a player has left this turn.
        /// </summary>
        public int Energy => _energy;

        /// <summary>
        /// The maximum energy the player can have this turn.
        /// </summary>
        public int EnergyCap => _energyCap;

        public readonly int playerIndex;
        public readonly string name;

        // Cards the player does not have access to until they are drawn.
        private readonly List<Card> _deck;

        // The cards the player has access to and can potentially play on the player's turn.
        private readonly List<Card> _hand;

        // Cards that have been discarded from the player's hand.
        private readonly List<Card> _discardPile;

        // The game entities the player controls on the board.
        private readonly List<Unit> _units;

        // The player's hero is the unit the player starts with. When it is defeated, the player loses.
        private Unit _hero;

        private int _energy;
        private int _energyCap;

        private readonly Game _game;
        private readonly PlayerData _playerData;

        // --------------------------------------------------------------------------------------------
        public Player(PlayerData playerData, Game game, int playerIndex)
        {
            this.playerIndex = playerIndex;
            this.name = playerData.name;

            _game = game;
            _playerData = playerData;

            // create the player's deck
            _deck = new List<Card>();
            foreach (string cardId in playerData.deckData.cardIdToCount.Keys)
            {
                CardData cardData = AppManager.Config.GetCardData(cardId);
                for (int i = 0; i < playerData.deckData.cardIdToCount[cardId]; i++)
                {
                    _deck.Add(Card.Create(this, cardData));
                }
            }

            // create the player's hand, but it will be empty until the player draws cards
            _hand = new List<Card>();

            // empty discard pile
            _discardPile = new List<Card>();

            // create the list of units and the player's hero, and add the hero to the list of units
            _units = new List<Unit>();

            // create the hero and place it on the board
            _hero = PlaceUnit(playerData.heroData, _game.board.GetHeroStartTile(playerIndex));
        }

        // --------------------------------------------------------------------------------------------
        public void StartTurn()
        {
            _energyCap++;
            _energyCap = Mathf.Min(_energyCap, AppManager.Config.MaxPlayerEnergy);

            _energy = _energyCap;

            PlayerTurnStarted?.Invoke(this, new PlayerEventArgs(this));
        }

        // --------------------------------------------------------------------------------------------
        public void EndTurn()
        {
            PlayerTurnEnded?.Invoke(this, new PlayerEventArgs(this));
        }

        // --------------------------------------------------------------------------------------------
        public void DrawCard()
        {
            Card nextCard = _deck[0];
            _deck.RemoveAt(0);
            _hand.Add(nextCard);
        }

        // --------------------------------------------------------------------------------------------
        public void Discard(Card card)
        {
            // first attempt to remove the card from the player's hand, then try to remove it from the players deck
            if (!_hand.Remove(card))
            {
                if (_deck.Remove(card))
                {
                    Debug.LogError($"the card {card.name} is not owned by this player");
                    return;
                }
            }

            _discardPile.Add(card);
        }

        // --------------------------------------------------------------------------------------------
        public void ShuffleDeck()
        {
            // TODO: might want to make this guaranteed to be deterministic
            for (int i = 0; i < _deck.Count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, _deck.Count - 1);
                Card temp = _deck[randomIndex];
                _deck[randomIndex] = _deck[i];
                _deck[i] = temp;
            }
        }

        // --------------------------------------------------------------------------------------------
        private Unit PlaceUnit(UnitData unitData, BoardTile boardTile)
        {
            Unit unit = new Unit(unitData, _game, this);
            unit.OccupyBoardTile(boardTile, true);

            // when placed, face the center of the board
            // TODO: is this the default behavior? should the player be able to choose the initial facing direction?
            unit.SetFacing(Unit.VectorToFacing(_game.board.CenterPos - unit.Transform.position), false);

            unit.OnTookDamage += OnUnitTookDamage;

            _units.Add(unit);

            return unit;
        }

        // --------------------------------------------------------------------------------------------
        private void OnUnitTookDamage(object sender, Unit.DamageEventArgs e)
        {
            if (!e.wasKilled)
            {
                return;
            }

            Unit senderUnit = sender as Unit;
            senderUnit.OnTookDamage -= OnUnitTookDamage;

            if (e.sourceUnit.Owner == this)
            {
                _units.Remove(e.sourceUnit);
            }

            if(e.targetUnit == Hero)
            {
                LoseGame();
            }
        }

        // --------------------------------------------------------------------------------------------
        private void LoseGame()
        {
            PlayerLost?.Invoke(this, new PlayerEventArgs(this));
        }

        // --------------------------------------------------------------------------------------------
        public class PlayerEventArgs : EventArgs
        {
            public readonly Player player;

            // --------------------------------------------------------------------------------------------
            public PlayerEventArgs(Player player)
            {
                this.player = player;
            }
        }
    }
}
