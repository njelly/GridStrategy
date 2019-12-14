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

        // --------------------------------------------------------------------------------------------
        public Game(List<PlayerData> playerDatas, int firstPlayerIndex)
        {
            gameCamera = GameCamera.Create();
            gameCamera.Render(AppManager.Transform);

            sun = SharpLight.Sun();
            sun.Render(AppManager.Transform);

            board = new Board(8, 8);
            board.Render(AppManager.Transform);

            _players = new List<Player>();
            for(int i = 0; i < playerDatas.Count; i++)
            {
                _players.Add(new Player(playerDatas[i], this, i));
            }

            _currentPlayerIndex = firstPlayerIndex;
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
