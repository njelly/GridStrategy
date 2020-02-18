using System.Collections.Generic;
using Tofunaut.SharpUnity;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class UnitUseSkillView : SharpGameObject
    {
        private static Vector3 Offset => new Vector3(2f, 2f, 0f);

        public Unit.EFacing CurrentFacing { get; set; }
        public BoardTile CurrentlyTargeting { get; set; }

        public Unit unit;

        private readonly Game _game;
        private List<BoardTile> _targetableTiles;

        // --------------------------------------------------------------------------------------------
        public UnitUseSkillView(Game game) : base("UnitUseSkillView")
        {
            _game = game;
        }

        // --------------------------------------------------------------------------------------------
        protected override void PostRender()
        {
            base.PostRender();

            _game.board.HighlightBoardTilesForUseSkill(unit.Skill);

            _targetableTiles = unit.Skill.GetTargetableTiles();
        }

        // --------------------------------------------------------------------------------------------
        protected override void Build() { }

        // --------------------------------------------------------------------------------------------
        public override void Destroy()
        {
            base.Destroy();

            _game.board.ClearAllBoardTileHighlights();
        }

        // --------------------------------------------------------------------------------------------
        public void TargetTowardTile(BoardTile boardTile)
        {
            // return if it is not the unit's owner's turn
            if(unit.Owner.playerIndex != _game.CurrentPlayer.playerIndex)
            {
                return;
            }

            // set CurrentlyTargeting to null and try to set it
            CurrentlyTargeting = null;
            int closestDistance = int.MaxValue;
            foreach(BoardTile targetableTile in _targetableTiles)
            {
                int distance = (boardTile.Coord - targetableTile.Coord).ManhattanDistance;
                if (distance > 1)
                {
                    continue;
                }

                if(distance < closestDistance)
                {
                    closestDistance = distance;
                    CurrentlyTargeting = targetableTile;
                }
            }

            GameObject.SetActive(CurrentlyTargeting != null);

            if (CurrentlyTargeting == null)
            {
                return;
            }

            Unit.EFacing facing = Unit.VectorToFacing(CurrentlyTargeting.LocalPosition - unit.BoardTile.LocalPosition);
            LocalRotation = Unit.FacingToRotation(facing);
            LocalPosition = unit.BoardTile.LocalPosition + (LocalRotation * (Vector3.right * Offset.x)) + new Vector3(0f, Offset.y, 0f);
        }
    }
}