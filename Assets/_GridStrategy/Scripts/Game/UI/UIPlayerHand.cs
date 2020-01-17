////////////////////////////////////////////////////////////////////////////////
//
//  UIPlayerHand (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for TofuUnity on 01/15/2020
//
////////////////////////////////////////////////////////////////////////////////

using Tofunaut.GridStrategy.UI;
using Tofunaut.SharpUnity.UI;

namespace Tofunaut.GridStrategy.Game.UI
{
    public class UIPlayerHand : UIGridStrategyView
    {
        private readonly Player _player;

        public UIPlayerHand(Player player) : base(UIPriorities.HUD)
        {
            _player = player;
        }

        protected override SharpUIBase BuildMainPanel()
        {
            throw new System.NotImplementedException();
        }
    }
}