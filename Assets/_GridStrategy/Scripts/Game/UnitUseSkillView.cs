using Tofunaut.SharpUnity;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    public class UnitUseSkillView : SharpSprite
    {
        public Unit unit;

        private readonly Game _game;

        public UnitUseSkillView(Game game) : base("FacingArrow", AppManager.AssetManager.Get<Sprite>(AssetPaths.Sprites.FacingArrow))
        {
            _game = game;
        }

        protected override void PostRender()
        {
            base.PostRender();

            _game.board.HighlightBoardTilesForUseSkill(unit.Skill);
        }

        public override void Destroy()
        {
            base.Destroy();

            _game.board.ClearAllBoardTileHighlights();
        }

        public void TargetTowardTile(BoardTile boardTile)
        {

        }
    }
}