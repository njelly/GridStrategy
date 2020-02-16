////////////////////////////////////////////////////////////////////////////////
//
//  Game (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/28/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using TofuCore;
using Tofunaut.GridStrategy.Game.UI;
using Tofunaut.SharpUnity;
using Tofunaut.TofuCore;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    /// <summary>
    /// Represents a single instance of a GridStrategy game.
    /// </summary>
    // TODO: Extend this class for NetworkedGame, LocalGame, etc.
    public class Game : UnitActionManager.IListener
    {
        public event EventHandler GameBegan;
        public event EventHandler GameFinished;
        public event EventHandler<PlayerActionEventArgs> PlayerActionCompleted;

        public bool HasBegun { get; private set; }
        public bool HasFinished { get; private set; }

        ///<summary>
        /// Get the player whose turn it is.
        ///</summary>
        public Player CurrentPlayer => _players[_currentPlayerIndex];
        public Player LocalPlayer => _players[_localPlayerIndex];
        public SerializedRandom Random => _random;

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
        private UnitActionManager _unitActionManager;
        private SerializedRandom _random;

        // --------------------------------------------------------------------------------------------
        public Game(List<PlayerData> playerDatas, int localPlayerIndex)
        {
            _currentPlayerIndex = 0;
            _localPlayerIndex = localPlayerIndex;

            // TODO: when networked, we need to share this in the room properites and then deserialize it on all clients
            _random = SerializedRandom.CreateNewInstance(1024);

            board = new Board(this, 5, 5);
            board.Render(AppManager.Transform);

            sun = SharpLight.Sun();
            sun.LocalRotation = Quaternion.Euler(125, 45, 0);
            sun.Render(AppManager.Transform);

            _players = new List<Player>();
            for (int i = 0; i < playerDatas.Count; i++)
            {
                // TODO: players need to have a synced initial seed
                Player player = new Player(playerDatas[i], this, i, (uint) i);
                player.PlayerLost += OnPlayerLost;
                _players.Add(player);
            }

            // This needs to happen before gameCamera, since it needs to register itself as a listener to UIWorldIteractionPanel
            _uiWorldInteractionPanel = UIWorldInteractionPanel.Create(this);

            _unitActionManager = new UnitActionManager(this, this);
            UIWorldInteractionPanel.AddListener(_unitActionManager);

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

            Debug.Log("ExecuteNextPlayerAction: " + _playerActions[_actionIndex].ToString());
            _playerActions[_actionIndex].Execute(this, onComplete);
        }

        // --------------------------------------------------------------------------------------------
        public Player GetPlayer(int index)
        {
            return _players[index];
        }

        // --------------------------------------------------------------------------------------------
        public List<Player> GetWinners()
        {
            List<Player> toReturn = new List<Player>();
            if(HasFinished)
            {
                foreach(Player player in _players)
                {
                    if(player.HasLost)
                    {
                        continue;
                    }

                    toReturn.Add(player);
                }
            }

            return toReturn;
        }

        // --------------------------------------------------------------------------------------------
        public void CleanUp()
        {
            UIWorldInteractionPanel.RemoveListener(_unitActionManager);

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

            board.ClearAllBoardTileHighlights();

            CurrentPlayer.EndTurn();

            // go through the players, skipping players that have lost
            int cycleCounter = 0;
            do
            {
                _currentPlayerIndex += 1;
                _currentPlayerIndex %= _players.Count;
                cycleCounter++;
            } while (CurrentPlayer.HasLost && cycleCounter <= _players.Count);
            if(cycleCounter >= _players.Count)
            {
                Debug.LogError("all players have lost, this shouldn't happen");
                return;
            }

            CurrentPlayer.StartTurn();
        }

        // --------------------------------------------------------------------------------------------
        private void OnPlayerLost(object sender, Player.PlayerEventArgs e)
        {
            int numPlayersLeft = 0;
            foreach(Player player in _players)
            {
                if(!player.HasLost)
                {
                    numPlayersLeft++;
                }
            }

            if(numPlayersLeft <= 1)
            {
                EndGame();
            }
        }

        // --------------------------------------------------------------------------------------------
        private void EndGame()
        {
            HasFinished = true;
            GameFinished?.Invoke(this, EventArgs.Empty);
        }

        public void OnPathSelected(Unit unit, IntVector2[] path)
        {
            QueueAction(new MoveAction(CurrentPlayer.playerIndex, unit.id, path), () => { });
        }

        public void OnSkillTargetSelected(Unit unit, Unit.EFacing facing, BoardTile target)
        {
            QueueAction(new UseSkillAction(CurrentPlayer.playerIndex, unit.id, facing, target.Coord), () => { });
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
