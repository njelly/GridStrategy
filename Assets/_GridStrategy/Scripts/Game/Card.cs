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
        public Player Owner { get { return _owner; } }

        public readonly CardData cardData;
        public readonly string name;
        public readonly int solidarityRequired;

        private readonly Game _game;

        private Player _owner;

        // --------------------------------------------------------------------------------------------
        public Card(CardData data, Game game, Player owner)
        {
            cardData = data;
            name = data.id;
            _owner = owner;
            solidarityRequired = data.energyRequired;

            _game = game;
        }

        // --------------------------------------------------------------------------------------------
        public List<BoardTile> GetPlayableTiles()
        {
            return GetPlayableTiles(_game, _owner, cardData);
        }

        // --------------------------------------------------------------------------------------------
        public static List<BoardTile> GetPlayableTiles(Game game, Player owner, CardData cardData)
        {
            List<BoardTile> toReturn = new List<BoardTile>();

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