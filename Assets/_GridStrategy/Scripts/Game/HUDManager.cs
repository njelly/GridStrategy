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

// --------------------------------------------------------------------------------------------
namespace Tofunaut.GridStrategy.Game.UI
{
    // --------------------------------------------------------------------------------------------
    public class HUDManager : SharpGameObject, UIContextMenuView.IListener
    {
        private Game _game;
        private UIBeginTurnBanner _beginTurnBanner;
        private UILeftPlayerPanel _localPlayerPanel;
        private UIConfirmationDialogView _confirmationDialog;
        private UIContextMenuView _contextMenuView;
        private List<UIRightPlayerPanel> _opponentPlayerPanels;

        // --------------------------------------------------------------------------------------------
        private HUDManager(Game game) : base("UIHUDManager")
        {
            _game = game;
        }

        // --------------------------------------------------------------------------------------------
        protected override void Build()
        {
            _beginTurnBanner = new UIBeginTurnBanner();
            _localPlayerPanel = new UILeftPlayerPanel(_game.LocalPlayer);
            _confirmationDialog = new UIConfirmationDialogView();
            _contextMenuView = new UIContextMenuView(this);

            //TODO: Put this in a vertical layout group for all opponent players
            _opponentPlayerPanels = new List<UIRightPlayerPanel>
            {
                // TODO: 1 is not guranteed to be an opponent player
                new UIRightPlayerPanel(_game.GetPlayer(1)),
            };
        }

        // --------------------------------------------------------------------------------------------
        protected override void PostRender()
        {
            base.PostRender();
            Player.PlayerTurnStarted += OnPlayerTurnStarted;
            _game.GameBegan += OnGameBegan;
        }

        // --------------------------------------------------------------------------------------------
        public override void Destroy()
        {
            base.Destroy();

            _contextMenuView.Hide();
            _localPlayerPanel.Hide();

            foreach (UIRightPlayerPanel opponentPanel in _opponentPlayerPanels)
            {
                opponentPanel.Hide();
            }

            Player.PlayerTurnStarted -= OnPlayerTurnStarted;
            _game.GameBegan -= OnGameBegan;
        }

        // --------------------------------------------------------------------------------------------
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

        // --------------------------------------------------------------------------------------------
        private void OnGameBegan(object sender, EventArgs e)
        {
            _localPlayerPanel.Show();
            _contextMenuView.Show();

            foreach (UIRightPlayerPanel opponentPanel in _opponentPlayerPanels)
            {
                opponentPanel.Show();
            }
        }

        // --------------------------------------------------------------------------------------------
        private void OnPlayerTurnStarted(object sender, Player.PlayerEventArgs e)
        {
            if (_beginTurnBanner.IsShowing)
            {
                _beginTurnBanner.Hide();
            }

            _beginTurnBanner.SetPlayerName(_game.CurrentPlayer.name);
            _beginTurnBanner.Show();
        }

        #region UIContextMenuView.IListener

        // --------------------------------------------------------------------------------------------
        public void OnEndTurnClicked()
        {
            _game.QueueAction(new EndTurnAction(_game.CurrentPlayer.playerIndex), () => { });
        }

        #endregion UIContextMenuView.IListener

        // --------------------------------------------------------------------------------------------
        public static HUDManager Create(Game game)
        {
            HUDManager toReturn = new HUDManager(game);

            return toReturn;
        }
    }
}
