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
    public class GameManager
    {
        public static GameManager Instance { get; private set; }

        public readonly GameCamera gameCamera;
        public readonly SharpLight sun;

        private int _currentPlayerIndex;
        private List<Player> _players;

        // --------------------------------------------------------------------------------------------
        public GameManager(List<PlayerData> players, int firstPlayerIndex)
        {
            if(Instance != null)
            {
                Debug.LogError("Only one instance of GameManager can exist at a time!");
                return;
            }

            Instance = this;

            gameCamera = GameCamera.Create();
            gameCamera.Render(AppManager.Transform);

            sun = SharpLight.Sun();
            sun.Render(AppManager.Transform);

            _players = new List<Player>();
            foreach(PlayerData playerData in players)
            {
                _players.Add(new Player(playerData));
            }

            _currentPlayerIndex = firstPlayerIndex;
        }

        // --------------------------------------------------------------------------------------------
        public void CleanUp()
        {
            Instance = null;
            gameCamera.Destroy();
            sun.Destroy();
        }
    }
}
