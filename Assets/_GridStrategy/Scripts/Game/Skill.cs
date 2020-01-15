////////////////////////////////////////////////////////////////////////////////
//
//  Skill (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 01/14/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using TofuCore;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class Skill
    {
        public SkillData.ETarget Target => _skillData.target;
        public int DamageDealt => _skillData.damageDealt;
        public int Range => _skillData.range;
        public Unit User => _unit;

        private readonly Game _game;
        private readonly Unit _unit;
        private readonly SkillData _skillData;

        // --------------------------------------------------------------------------------------------
        public Skill(SkillData skillData, Game game, Unit unit)
        {
            _skillData = skillData;
            _game = game;
            _unit = unit;
        }

        // --------------------------------------------------------------------------------------------
        public List<BoardTile> GetAffectedTiles(Unit.EFacing facing, IntVector2 startCoord)
        {
            BoardTile startTile = _game.board.GetTile(startCoord);
            List<BoardTile> toReturn = new List<BoardTile>();
            switch (_skillData.areaType)
            {
                case SkillData.EAreaType.Single:
                    toReturn.Add(startTile);
                    break;
                default:
                    throw new NotImplementedException($"GetTargetTiles not implemented for areaType {_skillData.areaType}");
            }

            return toReturn;
        }

        // --------------------------------------------------------------------------------------------
        public List<BoardTile> GetTargetableTiles()
        {
            List<BoardTile> toReturn = new List<BoardTile>();

            for(int x = 0; x < _game.board.width; x++)
            {
                for(int y = 0; y < _game.board.height; y++)
                {
                    BoardTile boardTile = _game.board.GetTile(x, y);
                    if(boardTile == null)
                    {
                        continue;
                    }

                    if (!IsTileValidTarget(boardTile))
                    {
                        continue;
                    }

                    if((boardTile.Coord - User.BoardTile.Coord).ManhattanDistance > _skillData.range)
                    {
                        continue;
                    }

                    toReturn.Add(boardTile);
                }
            }

            return toReturn;
        }

        // --------------------------------------------------------------------------------------------
        public bool IsTileValidTarget(BoardTile tile)
        {
            switch (Target)
            {
                case SkillData.ETarget.Ally:
                    return tile.ContainsAllyOf(User);
                case SkillData.ETarget.Enemy:
                    return tile.ContainsEnemyOf(User);
                case SkillData.ETarget.Tile:
                    return true;
                case SkillData.ETarget.Self:
                    return tile == User.BoardTile;
                case SkillData.ETarget.None:
                    return false;
                default:
                    Debug.LogError($"CanTargetTile not implemented for {Target}");
                    return false;
            }
        }
    }
}
