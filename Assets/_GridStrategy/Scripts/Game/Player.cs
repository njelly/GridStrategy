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

        public IReadOnlyCollection<Unit> Units { get { return _units.AsReadOnly(); } }
        public Unit Hero => _hero;
        public Hand Hand => _hand;
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
        private readonly Deck _deck;

        // Cards the player has already used.
        private readonly DiscardPile _discardPile;

        // Cards the player currently has access to.
        private readonly Hand _hand;

        // The game entities the player controls on the board.
        private readonly List<Unit> _units;

        // The player's hero is the unit the player starts with. When it is defeated, the player loses.
        private Unit _hero;

        private int _energy;
        private int _energyCap;

        private readonly Game _game;
        private readonly PlayerData _playerData;

        // --------------------------------------------------------------------------------------------
        public Player(PlayerData playerData, Game game, int playerIndex, uint deckSeed)
        {
            this.playerIndex = playerIndex;
            this.name = playerData.name;

            _game = game;
            _playerData = playerData;

            // create the player's deck
            _deck = new Deck(game, playerData.deckData, this, deckSeed);

            // create the player's discard pile
            _discardPile = new DiscardPile(_game, this);

            // create the player's hand
            _hand = new Hand(_game, this, _deck, _discardPile);

            // create the list of units and the player's hero, and add the hero to the list of units
            _units = new List<Unit>();

            // create the hero and place it on the board
            _hero = PlaceUnit(playerData.heroData, _game.board.GetHeroStartTile(playerIndex));

            _game.GameBegan += OnGameBegan;
        }

        // --------------------------------------------------------------------------------------------
        public void StartTurn()
        {
            _energyCap++;
            _energyCap = Mathf.Min(_energyCap, AppManager.Config.MaxPlayerEnergy);

            _energy = _energyCap;

            // draw a card if there are any cards left
            if(_deck.NumCardsLeft > 0)
            {
                if(_hand.Cards.Count < AppManager.Config.MaxHandSize)
                {
                    _hand.DrawCard();
                }
                else
                {
                    // TODO: lots of games have a penalty when the player has too many cards in their hand
                    // Do that here.
                }
            }

            PlayerTurnStarted?.Invoke(this, new PlayerEventArgs(this));
        }

        // --------------------------------------------------------------------------------------------
        public void EndTurn()
        {
            PlayerTurnEnded?.Invoke(this, new PlayerEventArgs(this));
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
        private void OnGameBegan(object sender, EventArgs e)
        {
            _game.GameBegan -= OnGameBegan;

            _game.GameFinished += OnGameFinished;

            for (int i = 0; i < AppManager.Config.StartHandSize; i++)
            {
                _hand.DrawCard();
            }
        }

        // --------------------------------------------------------------------------------------------
        private void OnGameFinished(object sender, EventArgs e)
        {
            _game.GameFinished -= OnGameFinished;
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
