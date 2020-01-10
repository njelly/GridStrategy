//////////////////////////////////////////////////////////////////////////////
//
//  UIHUDManager (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 01/03/2020
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Tofunaut.GridStrategy.UI;
using Tofunaut.SharpUnity;
using Tofunaut.SharpUnity.UI;

namespace Tofunaut.GridStrategy.Game.UI
{
    public class HUDManager : SharpGameObject
    {
        private Game _game;
        private UIBeginTurnBanner _beginTurnBanner;
        private UILeftPlayerPanel _localPlayerPanel;
        private UIConfirmationDialogView _confirmationDialog;
        private List<UIRightPlayerPanel> _opponentPlayerPanels;

        private HUDManager(Game game) : base("UIHUDManager")
        {
            _game = game;
        }

        protected override void Build()
        {
            _beginTurnBanner = new UIBeginTurnBanner();
            _localPlayerPanel = new UILeftPlayerPanel(_game.LocalPlayer);
            _confirmationDialog = new UIConfirmationDialogView();

            //TODO: Put this in a vertical layout group for all opponent players
            _opponentPlayerPanels = new List<UIRightPlayerPanel>
            {
                // TODO: 1 is not guranteed to be an opponent player
                new UIRightPlayerPanel(_game.GetPlayer(1)),
            };
        }

        protected override void PostRender()
        {
            base.PostRender();
            _game.PlayerTurnStarted += OnPlayerTurnStart;
            _game.GameBegan += OnGameBegan;
        }

        public override void Destroy()
        {
            base.Destroy();

            _localPlayerPanel.Hide();

            foreach (UIRightPlayerPanel opponentPanel in _opponentPlayerPanels)
            {
                opponentPanel.Hide();
            }

            _game.PlayerTurnStarted -= OnPlayerTurnStart;
            _game.GameBegan -= OnGameBegan;
        }

        public void ShowConfirmationDialog(Action onConfirm, Action onCancel)
        {
            _confirmationDialog.OnOKClicked = () =>
            {
                onConfirm?.Invoke();
                _confirmationDialog.Hide();
            };
            _confirmationDialog.OnCancelClicked = () =>
            {
                onCancel?.Invoke();
                _confirmationDialog.Hide();
            };

            _confirmationDialog.Show();
        }

        private void OnGameBegan(object sender, EventArgs e)
        {
            _localPlayerPanel.Show();

            foreach(UIRightPlayerPanel opponentPanel in _opponentPlayerPanels)
            {
                opponentPanel.Show();
            }
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
