////////////////////////////////////////////////////////////////////////////////
//
//  GameManager (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/28/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Tofunaut.GridStrategy.Game.UI;
using Tofunaut.SharpUnity;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    /// <summary>
    /// Represents a single instance of a GridStrategy game.
    /// </summary>
    // TODO: Extend this class for NetworkedGame, LocalGame, etc.
    public class Game
    {
        public event EventHandler GameBegan;
        public event EventHandler<PlayerActionEventArgs> PlayerActionCompleted;

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
            _unitPathSelectionManager.OnBoardTileSelected += OnBoardTileSelected;
            _unitPathSelectionManager.OnUnitSelected += OnUnitSelected;
            UIWorldInteractionPanel.AddListener(_unitPathSelectionManager);

            gameCamera = GameCamera.Create(this, -67.5f, _players[_currentPlayerIndex].Hero.GameObject.transform.position);
            gameCamera.Render(AppManager.Transform);

            _playerActions = new List<PlayerAction>();
            _actionIndex = -1;

            _hudManager = HUDManager.Create(this);
            _hudManager.Render(AppManager.Transform);
        }

        // --------------------------------------------------------------------------------------------
        public void BeginGame()
        {
            if (HasBegun)
            {
                Debug.LogError("the game has already begun");
                return;
            }

            HasBegun = true;

            GameBegan?.Invoke(this, EventArgs.Empty);

            CurrentPlayer.StartTurn();
        }

        // --------------------------------------------------------------------------------------------
        public void QueueAction(PlayerAction action, Action onComplete)
        {
            _playerActions.Add(action);

            // TODO: a networked game would do an RPC call and then maybe wait for a confirmation from clients
            ExecuteNextPlayerAction(() =>
            {
                onComplete?.Invoke();
                PlayerActionCompleted?.Invoke(this, new PlayerActionEventArgs(action));
            });
        }

        // --------------------------------------------------------------------------------------------
        protected void ExecuteNextPlayerAction(Action onComplete)
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
            _unitPathSelectionManager.OnBoardTileSelected -= OnBoardTileSelected;
            _unitPathSelectionManager.OnUnitSelected -= OnUnitSelected;
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
            if(!HasBegun)
            {
                Debug.LogError("Can't end turn, the game has not yet begun.");
                return;
            }

            if(HasFinished)
            {
                Debug.Log("Can't end turn, the game is over");
                return;
            }

            CurrentPlayer.EndTurn();

            _currentPlayerIndex += 1;
            _currentPlayerIndex %= _players.Count;

            CurrentPlayer.StartTurn();
        }

        // --------------------------------------------------------------------------------------------
        private void OnPathSelected(object sender, UnitPathSelectionManager.PathEventArgs e)
        {
            _hudManager.ShowConfirmationDialog(() =>
            {
                _unitPathSelectionManager.Enabled = false;
                QueueAction(new MoveAction(_currentPlayerIndex, e.unitView.Unit.id, e.path), () =>
                {
                    _unitPathSelectionManager.Enabled = true;
                });
            }, () =>
            {
                _unitPathSelectionManager.ClearSelection();
            });
        }

        // --------------------------------------------------------------------------------------------
        private void OnBoardTileSelected(object sender, UnitPathSelectionManager.BoardTileEventArgs e)
        {
            Debug.Log("selected tile: " + e.boardTileView.BoardTile.Coord.ToString());
        }

        // --------------------------------------------------------------------------------------------
        private void OnUnitSelected(object sender, UnitPathSelectionManager.UnitEventArgs e)
        {
            Debug.Log("selected unit: " + e.unitView.Unit.id);
        }

        // --------------------------------------------------------------------------------------------
        public class PlayerActionEventArgs : EventArgs
        {
            public readonly PlayerAction playerAction;

            // --------------------------------------------------------------------------------------------
            public PlayerActionEventArgs(PlayerAction playerAction)
            {
                this.playerAction = playerAction;
            }
        }
    }
}
