////////////////////////////////////////////////////////////////////////////////
//
//  GameManager (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/28/2019
//
////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using TofuCore;
using Tofunaut.GridStrategy.Game.UI;
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
            _currentPlayerIndex = firstPlayerIndex;

            board = new Board(8, 8);
            board.Render(AppManager.Transform);

            sun = SharpLight.Sun();
            sun.LocalRotation = Quaternion.Euler(125, 45, 0);
            sun.Render(AppManager.Transform);

            _players = new List<Player>();
            for(int i = 0; i < playerDatas.Count; i++)
            {
                _players.Add(new Player(playerDatas[i], this, i));
            }

            gameCamera = GameCamera.Create(this, -67.5f, _players[_currentPlayerIndex].Hero.GameObject.transform.position);
            gameCamera.Render(AppManager.Transform);

            _playerActions = new List<PlayerAction>();
            _actionIndex = -1;

            // TEST
            QueueAction(new MoveAction(_currentPlayerIndex, _players[_currentPlayerIndex].Hero.id, new[] { new IntVector2(1, 0) }));
            ExecuteNextPlayerAction();
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

            PlayerAction nextAction = _playerActions[_actionIndex];

            switch (nextAction.type)
            {
                case PlayerAction.EType.Invalid:
                    Debug.LogError($"PlayerAction {_actionIndex} has the type Invalid!");
                    break;
                case PlayerAction.EType.MoveUnit:
                    MoveAction moveAction = nextAction as MoveAction;
                    Unit toMove = Unit.GetUnit(moveAction.unitId);
                    toMove.Move(moveAction.path, false);
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
