////////////////////////////////////////////////////////////////////////////////
//
//  GameManager (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/28/2019
//
////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using Tofunaut.SharpUnity;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class Game
    {
        public readonly GameCamera gameCamera;
        public readonly SharpLight sun;
        public readonly Board board;

        private int _currentPlayerIndex;
        private List<Player> _players;
        private List<PlayerAction> _playerActions;
        private int _actionIndex;

        // --------------------------------------------------------------------------------------------
        public Game(List<PlayerData> playerDatas, int firstPlayerIndex)
        {
            gameCamera = GameCamera.Create();
            gameCamera.Render(AppManager.Transform);

            sun = SharpLight.Sun();
            sun.LocalRotation = Quaternion.Euler(125, 45, 0);
            sun.Render(AppManager.Transform);

            board = new Board(8, 8);
            board.Render(AppManager.Transform);

            _players = new List<Player>();
            for(int i = 0; i < playerDatas.Count; i++)
            {
                _players.Add(new Player(playerDatas[i], this, i));
            }

            _currentPlayerIndex = firstPlayerIndex;

            _playerActions = new List<PlayerAction>();
            _actionIndex = -1;
        }

        // --------------------------------------------------------------------------------------------
        public void QueueAction(PlayerAction action)
        {
            _playerActions.Add(action);
        }

        // --------------------------------------------------------------------------------------------
        public void ExecuteNextPlayerAction()
        {
            _actionIndex++;

            // first verify that this action can be executed based on the current game state
            if(!_playerActions[_actionIndex].IsValid(this))
            {
                Debug.LogError($"PlayerAction {_actionIndex} cannot be executed.");
                return;
            }

            switch(_playerActions[_actionIndex].type)
            {
                case PlayerAction.EType.Invalid:
                    Debug.LogError($"PlayerAction {_actionIndex} has the type Invalid!");
                    break;

            }
        }

        // --------------------------------------------------------------------------------------------
        public void CleanUp()
        {
            gameCamera.Destroy();
            sun.Destroy();
            board.Destroy();
        }
    }
}
