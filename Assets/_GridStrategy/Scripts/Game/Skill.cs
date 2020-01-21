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
        public Unit User => _user;

        private readonly Game _game;
        private readonly Unit _user;
        private readonly SkillData _skillData;

        // --------------------------------------------------------------------------------------------
        public Skill(SkillData skillData, Game game, Unit user)
        {
            _skillData = skillData;
            _game = game;
            _user = user;
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
            return GetTargetableTiles(_skillData, _game, _user);
        }

        // --------------------------------------------------------------------------------------------
        public bool IsTileValidTarget(BoardTile tile)
        {
            return IsTileValidTarget(_skillData, _user, tile);
        }

        // --------------------------------------------------------------------------------------------
        public static List<BoardTile> GetTargetableTiles(SkillData skillData, Game game, Unit user)
        {
            List<BoardTile> toReturn = new List<BoardTile>();

            for (int x = 0; x < game.board.width; x++)
            {
                for (int y = 0; y < game.board.height; y++)
                {
                    BoardTile boardTile = game.board.GetTile(x, y);
                    if (boardTile == null)
                    {
                        continue;
                    }

                    if (!IsTileValidTarget(skillData, user, boardTile))
                    {
                        continue;
                    }

                    if ((boardTile.Coord - user.BoardTile.Coord).ManhattanDistance > skillData.range)
                    {
                        continue;
                    }

                    toReturn.Add(boardTile);
                }
            }

            return toReturn;
        }

        // --------------------------------------------------------------------------------------------
        public static bool IsTileValidTarget(SkillData skillData, Unit user, BoardTile tile)
        {
            switch (skillData.target)
            {
                case SkillData.ETarget.Ally:
                    return tile.Occupant?.IsAllyOf(user) ?? false;
                case SkillData.ETarget.Enemy:
                    return tile.Occupant?.IsEnemyOf(user) ?? false;
                case SkillData.ETarget.Tile:
                    return true;
                case SkillData.ETarget.Self:
                    return tile == user.BoardTile;
                case SkillData.ETarget.None:
                    return false;
                default:
                    Debug.LogError($"CanTargetTile not implemented for {skillData.target}");
                    return false;
            }
        }
    }
}
