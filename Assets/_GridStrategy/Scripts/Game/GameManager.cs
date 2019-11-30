////////////////////////////////////////////////////////////////////////////////
//
//  GameManager (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/28/2019
//
////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using Tofunaut.SharpUnity;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class GameManager
    {
        public readonly GameCamera gameCamera;
        public readonly SharpLight sun;

        private int _currentPlayerIndex;
        private List<Player> _players;

        // --------------------------------------------------------------------------------------------
        public GameManager(List<PlayerData> players, int firstPlayerIndex)
        {
            gameCamera = GameCamera.Create();
            gameCamera.Render(AppManager.Transform);

            sun = SharpLight.Sun();
            sun.Render(AppManager.Transform);

            _players = new List<Player>();
            foreach(PlayerData playerData in players)
            {
                //_players.Add(new Player())
            }

            _currentPlayerIndex = firstPlayerIndex;
        }

        // --------------------------------------------------------------------------------------------
        public void CleanUp()
        {
            gameCamera.Destroy();
            sun.Destroy();
        }
    }
}
