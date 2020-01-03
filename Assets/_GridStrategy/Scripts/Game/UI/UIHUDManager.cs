//////////////////////////////////////////////////////////////////////////////
//
//  UIHUDManager (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 01/03/2020
//
////////////////////////////////////////////////////////////////////////////////

using Tofunaut.GridStrategy.UI;
using Tofunaut.SharpUnity.UI;

namespace Tofunaut.GridStrategy.Game.UI
{
    public class UIHUDManager : GridStrategyUIView
    {
        private Game _game;

        public UIHUDManager(Game game)
        {
            _game = game;
        }

        protected override SharpUIBase BuildMainPanel()
        {
            throw new System.NotImplementedException();
        }
    }
}