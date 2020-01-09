////////////////////////////////////////////////////////////////////////////////
//
//  GameManager (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/28/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using TofuCore;
using Tofunaut.GridStrategy.Game.UI;
using Tofunaut.SharpUnity;
using Tofunaut.UnityUtils;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class Game
    {
        public event EventHandler GameBegan;
        public event EventHandler PlayerTurnStarted;
        public event EventHandler PlayerTurnEnded;

        public bool HasBegun { get; private set; }
        public bool HasFinished { get; private set; }

        ///<summary>
        /// Get the player whose turn it is.
        ///</summary>
        public Player CurrentPlayer => _players[_currentPlayerIndex];
        public Player LocalPlayer => _players[_localPlayerIndex];

        public readonly GameCamera gameCamera;
        public readonly SharpLight sun;
        public readonly Board board;

        private readonly int _localPlayerIndex;

        private int _currentPlayerIndex;
        private List<Player> _players;
        private List<PlayerAction> _playerActions;
        private int _actionIndex;
        private HUDManager _hudManager;
        private UIWorldInteractionPanel _uiWorldInteractionPanel;
        private UnitPathSelectionManager _unitPathSelectionManager;

        // --------------------------------------------------------------------------------------------
        public Game(List<PlayerData> playerDatas, int localPlayerIndex)
        {
            _currentPlayerIndex = 0;
            _localPlayerIndex = localPlayerIndex;

            board = new Board(8, 8);
            board.Render(AppManager.Transform);

            sun = SharpLight.Sun();
            sun.LocalRotation = Quaternion.Euler(125, 45, 0);
            sun.Render(AppManager.Transform);

            _players = new List<Player>();
            for (int i = 0; i < playerDatas.Count; i++)
            {
                _players.Add(new Player(playerDatas[i], this, i));
            }

            // This needs to happen before gameCamera, since it needs to register itself as a listener to UIWorldIteractionPanel
            _uiWorldInteractionPanel = UIWorldInteractionPanel.Create(this);

            _unitPathSelectionManager = new UnitPathSelectionManager(this);
            _unitPathSelectionManager.OnPathSelected += OnPathSelected;
            UIWorldInteractionPanel.AddListener(_unitPathSelectionManager);

            gameCamera = GameCamera.Create(this, -67.5f, _players[_currentPlayerIndex].Hero.GameObject.transform.position);
            gameCamera.Render(AppManager.Transform);

            _playerActions = new List<PlayerAction>();
            _actionIndex = -1;

            _hudManager = HUDManager.Create(this);
            _hudManager.Render(AppManager.Transform);
        }

        public void BeginGame()
        {
            if (HasBegun)
            {
                Debug.LogError("the game has already begun");
                return;
            }

            HasBegun = true;

            GameBegan?.Invoke(this, EventArgs.Empty);

            PlayerTurnStarted?.Invoke(this, EventArgs.Empty);
        }

        // --------------------------------------------------------------------------------------------
        public void QueueAction(PlayerAction action)
        {
            _playerActions.Add(action);
        }

        // --------------------------------------------------------------------------------------------
        public void ExecuteNextPlayerAction(Action onComplete)
        {
            _actionIndex++;

            // first verify that this action can be executed based on the current game state
            if (!_playerActions[_actionIndex].IsValid(this))
            {
                Debug.LogError($"PlayerAction {_actionIndex} cannot be executed.");
                return;
            }

            _playerActions[_actionIndex].Execute(this, onComplete);
        }

        // --------------------------------------------------------------------------------------------
        public Player GetPlayer(int index)
        {
            return _players[index];
        }

        // --------------------------------------------------------------------------------------------
        public void CleanUp()
        {
            _unitPathSelectionManager.OnPathSelected -= OnPathSelected;
            UIWorldInteractionPanel.RemoveListener(_unitPathSelectionManager);

            gameCamera.Destroy();
            sun.Destroy();
            board.Destroy();
            _hudManager.Destroy();
            _uiWorldInteractionPanel.Destroy();
        }

        // --------------------------------------------------------------------------------------------
        public void EndTurn()
        {
            PlayerTurnEnded?.Invoke(this, EventArgs.Empty);
            _currentPlayerIndex += 1;
            _currentPlayerIndex %= _players.Count;
            PlayerTurnStarted?.Invoke(this, EventArgs.Empty);
        }

        // --------------------------------------------------------------------------------------------
        private void OnPathSelected(object sender, UnitPathSelectionManager.PathEventArgs e)
        {
            QueueAction(new MoveAction(_currentPlayerIndex, e.unitView.Unit.id, e.path));

            _unitPathSelectionManager.Enabled = false;
            ExecuteNextPlayerAction(() =>
            {
                _unitPathSelectionManager.Enabled = true;
            });
        }
    }
}
