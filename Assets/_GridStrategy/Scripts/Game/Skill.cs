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

namespace Tofunaut.GridStrategy.Game
{
    public class Skill
    {
        public SkillData.ETarget Target => _skillData.target;
        public int DamageDealt => _skillData.damageDealt;
        public Unit User => _unit;

        private readonly Game _game;
        private readonly Unit _unit;
        private readonly SkillData _skillData;

        public Skill(SkillData skillData, Game game, Unit unit)
        {
            _skillData = skillData;
            _game = game;
            _unit = unit;
        }

        public List<BoardTile> GetTargetTiles(Unit.EFacing facing, IntVector2 startCoord)
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
    }
}
