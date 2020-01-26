////////////////////////////////////////////////////////////////////////////////
//
//  Card (c) 2019 Tofunaut
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
    public class Card
    {
        private static int _idCounter = 0;
        private static List<Card> _idToCard = new List<Card>();

        public Player Owner { get { return _owner; } }

        public readonly CardData cardData;
        public readonly string name;
        public readonly int energyRequired;
        public readonly int id;

        private readonly Game _game;

        private Player _owner;

        // --------------------------------------------------------------------------------------------
        public Card(CardData data, Game game, Player owner)
        {
            cardData = data;
            name = data.id;
            _owner = owner;
            energyRequired = data.energyRequired;

            _game = game;

            id = _idCounter;
            _idCounter++;
            _idToCard.Add(this);
        }

        // --------------------------------------------------------------------------------------------
        public List<BoardTile> GetPlayableTiles()
        {
            return GetPlayableTiles(_game, _owner, cardData);
        }

        // --------------------------------------------------------------------------------------------
        public bool CanPlayOnTile(BoardTile boardTile)
        {
            return GetPlayableTiles().Contains(boardTile);
        }

        // --------------------------------------------------------------------------------------------
        public static List<BoardTile> GetPlayableTiles(Game game, Player owner, CardData cardData)
        {
            List<BoardTile> toReturn = new List<BoardTile>();

            if(cardData.energyRequired > owner.Energy)
            {
                // return empty list because this card is too expensive to be palyed.
                return toReturn;
            }

            if (!string.IsNullOrEmpty(cardData.useSkillId))
            {
                return Skill.GetTargetableTiles(AppManager.Config.GetSkillData(cardData.useSkillId), game, owner.Hero);
            }

            if (!string.IsNullOrEmpty(cardData.spawnUnitId))
            {
                UnitData unitData = AppManager.Config.GetUnitData(cardData.spawnUnitId);
                for (int x = 0; x < game.board.width; x++)
                {
                    for (int y = 0; y < game.board.height; y++)
                    {
                        BoardTile boardTile = game.board.GetTile(x, y);
                        if (Unit.CanSpawnOnTile(unitData, boardTile, owner.Hero))
                        {
                            toReturn.Add(boardTile);
                        }
                    }
                }

                return toReturn;
            }

            return toReturn;
        }

        // --------------------------------------------------------------------------------------------
        public static bool CanPlayOnTile(Game game, Player owner, CardData cardData, BoardTile boardTile)
        {
            return GetPlayableTiles(game, owner, cardData).Contains(boardTile);
        }

        // --------------------------------------------------------------------------------------------
        public static Card GetCard(int id)
        {
            if (id >= _idToCard.Count)
            {
                Debug.LogError($"no unit for id {id}");
            }

            return _idToCard[id];
        }

        // --------------------------------------------------------------------------------------------
        public class CardEventArgs : EventArgs
        {
            public readonly Card card;

            public CardEventArgs(Card card)
            {
                this.card = card;
            }
        }
    }
}