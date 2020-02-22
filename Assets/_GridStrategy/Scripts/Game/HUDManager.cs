////////////////////////////////////////////////////////////////////////////////
//
//  UIHUDManager (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 01/03/2020
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using TofuCore;
using Tofunaut.GridStrategy.UI;
using Tofunaut.SharpUnity;
using Tofunaut.SharpUnity.UI;
using UnityEngine;
using UnityEngine.EventSystems;

// --------------------------------------------------------------------------------------------
namespace Tofunaut.GridStrategy.Game.UI
{
    // --------------------------------------------------------------------------------------------
    public class HUDManager : SharpGameObject, UIContextMenuView.IListener, UIUseSkillView.IListener, UIPlayerHand.IListener
    {
        private Game _game;
        private UIBeginTurnBanner _beginTurnBanner;
        private UIConfirmationDialogView _confirmationDialog;
        private UIContextMenuView _contextMenuView;
        private UIUseSkillView _unitOptionsView;
        private Dictionary<Player, UILeftPlayerPanel> _playerToPlayerPanels;
        private UIPlayerHand _localPlayerHand;

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
            _unitOptionsView = new UIUseSkillView(this, _game);
            _playerToPlayerPanels = new Dictionary<Player, UILeftPlayerPanel>();
            _playerToPlayerPanels.Add(_game.LocalPlayer, new UILeftPlayerPanel(_game.LocalPlayer));
            _localPlayerHand = new UIPlayerHand(this, _game, _game.LocalPlayer);


            // TODO: Put this in a vertical layout group for all opponent players
            // TODO: 1 is not guranteed to be an opponent player
            _playerToPlayerPanels.Add(_game.GetPlayer(1), new UIRightPlayerPanel(_game.GetPlayer(1)));
        }

        // --------------------------------------------------------------------------------------------
        protected override void PostRender()
        {
            base.PostRender();

            foreach(Player player in _playerToPlayerPanels.Keys)
            {
                player.Hero.OnTookDamage += OnUnitTookDamage;
                player.PlayerTurnStarted += OnPlayerTurnStarted;
                player.PlayerPlayedCard += OnPlayerPlayedCard;
                player.PlayerSourceChanged += OnPlayerSourceChanged;
            }

            _game.GameBegan += OnGameBegan;
            _game.GameFinished += OnGameFinished;
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

            foreach (Player player in _playerToPlayerPanels.Keys)
            {
                player.Hero.OnTookDamage -= OnUnitTookDamage;
                player.PlayerTurnStarted -= OnPlayerTurnStarted;
                player.PlayerPlayedCard -= OnPlayerPlayedCard;
                player.PlayerSourceChanged -= OnPlayerSourceChanged;
            }

            _localPlayerHand.Hide();

            _game.GameBegan -= OnGameBegan;
            _game.GameFinished -= OnGameFinished;
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
            if (_unitOptionsView.IsShowing)
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

            _localPlayerHand.Show();
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

            if (_unitOptionsView.IsShowing)
            {
                _unitOptionsView.Hide();
            }
            
            _playerToPlayerPanels[e.player].SetEnergy(e.player.Energy, e.player.EnergyCap);
        }

        // --------------------------------------------------------------------------------------------
        private void OnPlayerPlayedCard(object sender, Player.PlayerEventArgs e)
        {
            _playerToPlayerPanels[e.player].SetEnergy(e.player.Energy, e.player.EnergyCap);
        }

        // --------------------------------------------------------------------------------------------
        private void OnPlayerSourceChanged(object sender, Player.PlayerEventArgs e)
        {
            _playerToPlayerPanels[e.player].SetSource(e.player.Source);
        }

        // --------------------------------------------------------------------------------------------
        private void OnUnitTookDamage(object sender, Unit.DamageEventArgs e)
        {
            foreach(Player player in _playerToPlayerPanels.Keys)
            {
                if(player.Hero != e.targetUnit)
                {
                    continue;
                }

                _playerToPlayerPanels[player].SetHealth(e.newHealth, e.targetUnit.MaxHealth);
            }
        }

        // --------------------------------------------------------------------------------------------
        private void OnGameFinished(object sender, EventArgs e)
        {
            _contextMenuView.Hide();
        }

        #region UIContextMenuView.IListener

        // --------------------------------------------------------------------------------------------
        public void OnEndTurnClicked()
        {
            _game.QueueAction(new EndTurnAction(_game.CurrentPlayer.playerIndex), () => { });
        }

        #endregion UIContextMenuView.IListener

        #region UIUnitOptionsView.IListener

        public void OnUseSkillConfirmed(Unit unit, Unit.EFacing facing, IntVector2 targetCoord)
        {
            _unitOptionsView.Hide();
            _game.QueueAction(new UseSkillAction(_game.CurrentPlayer.playerIndex, unit.id, facing, targetCoord), () => { });
        }

        #endregion UIUnitOptionsVIew.IListener

        #region UIPlayerHand.IListener

        // --------------------------------------------------------------------------------------------
        public void OnPlayerDraggedOutCard(Card card)
        {
            _game.board.HighlightBoardTilesForPlayCard(card);
        }

        // --------------------------------------------------------------------------------------------
        public void OnPlayerReleasedCard(Card card, PointerEventData pointerEventData)
        {
            _game.board.ClearAllBoardTileHighlights();

            if(_game.board.RaycastToPlane(_game.gameCamera.ScreenPointToRay(pointerEventData.position),out Vector3 worldPosition)) 
            {
                BoardTile boardTile = _game.board.GetBoardTileAtPosition(worldPosition);
                if(boardTile != null && Card.CanPlayOnTile(_game, card.Owner, card.cardData, boardTile))
                {
                    _game.QueueAction(new PlayCardAction(card.Owner.playerIndex, card.id, boardTile.Coord), () => { });
                }
            }
        }

        #endregion UIPlayerHand.IListener

        // --------------------------------------------------------------------------------------------
        public static HUDManager Create(Game game)
        {
            HUDManager toReturn = new HUDManager(game);

            return toReturn;
        }
    }
}
