using Tofunaut.SharpUnity;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class UnitUseSkillView : SharpSprite
    {
        private static Vector3 Offset => new Vector3(2f, 2f, 0f);

        public Unit.EFacing CurrentFacing { get; set; }

        public Unit unit;

        private readonly Game _game;

        // --------------------------------------------------------------------------------------------
        public UnitUseSkillView(Game game) : base("FacingArrow", AppManager.AssetManager.Get<Sprite>(AssetPaths.Sprites.FacingArrow))
        {
            _game = game;
            LocalScale = Vector3.one * 4;
        }

        // --------------------------------------------------------------------------------------------
        protected override void PostRender()
        {
            base.PostRender();

            _game.board.HighlightBoardTilesForUseSkill(unit.Skill);
        }

        // --------------------------------------------------------------------------------------------
        public override void Destroy()
        {
            base.Destroy();

            _game.board.ClearAllBoardTileHighlights();
        }

        // --------------------------------------------------------------------------------------------
        public void TargetTowardTile(BoardTile boardTile)
        {
            GameObject.SetActive(boardTile != unit.BoardTile);

            Unit.EFacing facing = Unit.VectorToFacing(boardTile.LocalPosition - unit.LocalPosition);
            LocalRotation = Unit.FacingToRotation(facing);

            LocalPosition = unit.BoardTile.LocalPosition + (LocalRotation * (Vector3.right * Offset.x)) + new Vector3(0f, Offset.y, 0f);
        }
    }
}