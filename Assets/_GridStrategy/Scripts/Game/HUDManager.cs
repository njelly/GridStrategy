//////////////////////////////////////////////////////////////////////////////
//
//  UIHUDManager (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 01/03/2020
//
////////////////////////////////////////////////////////////////////////////////

using System;
using Tofunaut.GridStrategy.UI;
using Tofunaut.SharpUnity;
using Tofunaut.SharpUnity.UI;

namespace Tofunaut.GridStrategy.Game.UI
{
    public class HUDManager : SharpGameObject
    {
        private Game _game;
        private UIBeginTurnBanner _beginTurnBanner;

        private HUDManager(Game game) : base("UIHUDManager")
        {
            _game = game;
        }

        protected override void Build()
        {
            _beginTurnBanner = new UIBeginTurnBanner();
        }

        protected override void PostRender()
        {
            base.PostRender();
            _game.PlayerTurnStart += OnPlayerTurnStart;
        }

        public override void Destroy()
        {
            base.Destroy();
            _game.PlayerTurnStart -= OnPlayerTurnStart;
        }

        private void OnPlayerTurnStart(object sender, EventArgs e)
        {
            if (_beginTurnBanner.IsShowing)
            {
                _beginTurnBanner.Hide();
            }

            _beginTurnBanner.SetPlayerName(_game.CurrentPlayer.name);
            _beginTurnBanner.Show();
        }

        public static HUDManager Create(Game game)
        {
            HUDManager toReturn = new HUDManager(game);

            return toReturn;
        }
    }
}