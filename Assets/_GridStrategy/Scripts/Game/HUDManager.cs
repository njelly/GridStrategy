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
    public class HUDManager : SharpGameObject, UIContextMenuView.IListener, UIUnitOptionsView.IListener
    {
        private Game _game;
        private UIBeginTurnBanner _beginTurnBanner;
        private UIConfirmationDialogView _confirmationDialog;
        private UIContextMenuView _contextMenuView;
        private UIUnitOptionsView _unitOptionsView;
        private Dictionary<Player, UILeftPlayerPanel> _playerToPlayerPanels;

        // --------------------------------------------------------------------------------------------
        private HUDManager(Game game) : base("UIHUDManager")
        {
            _game = game;
        }

        // --------------------------------------------------------------------------------------------
        protected override void Build()
        {
            _beginTurnBanner = new UIBeginTurnBanner();
            _confirmationDialog = new UIConfirmationDialogView();
            _contextMenuView = new UIContextMenuView(this);
            _unitOptionsView = new UIUnitOptionsView(this, _game.gameCamera);
            _playerToPlayerPanels = new Dictionary<Player, UILeftPlayerPanel>();
            _playerToPlayerPanels.Add(_game.LocalPlayer, new UILeftPlayerPanel(_game.LocalPlayer));


            //TODO: Put this in a vertical layout group for all opponent players
            // TODO: 1 is not guranteed to be an opponent player
            _playerToPlayerPanels.Add(_game.GetPlayer(1), new UIRightPlayerPanel(_game.GetPlayer(1)));
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

            foreach (UILeftPlayerPanel playerPanel in _playerToPlayerPanels.Values)
            {
                playerPanel.Hide();
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
        public void ShowUnitOptions(Unit unit)
        {
            if(_unitOptionsView.IsShowing)
            {
                _unitOptionsView.Hide();
            }

            _unitOptionsView.FollowUnit(unit);
            _unitOptionsView.Show();
        }

        // --------------------------------------------------------------------------------------------
        private void OnGameBegan(object sender, EventArgs e)
        {
            _contextMenuView.Show();

            foreach (UILeftPlayerPanel playerPanel in _playerToPlayerPanels.Values)
            {
                playerPanel.Show();
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

            if(_unitOptionsView.IsShowing)
            {
                _unitOptionsView.Hide();
            }

            foreach (Player player in _playerToPlayerPanels.Keys)
            {
                _playerToPlayerPanels[player].SetEnergy(player.Energy, player.EnergyCap);
            }
        }

        #region UIContextMenuView.IListener

        // --------------------------------------------------------------------------------------------
        public void OnEndTurnClicked()
        {
            _game.QueueAction(new EndTurnAction(_game.CurrentPlayer.playerIndex), () => { });
        }

        #endregion UIContextMenuView.IListener

        #region UIUnitOptionsView.IListener

        public void OnUseSkillClicked(Unit unit)
        {
            _unitOptionsView.Hide();
            _game.QueueAction(new UseSkillAction(_game.CurrentPlayer.playerIndex, unit.id), () => { });
        }

        #endregion UIUnitOptionsVIew.IListener

        // --------------------------------------------------------------------------------------------
        public static HUDManager Create(Game game)
        {
            HUDManager toReturn = new HUDManager(game);

            return toReturn;
        }
    }
}
