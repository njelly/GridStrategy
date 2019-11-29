////////////////////////////////////////////////////////////////////////////////
//
//  GameManager (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/28/2019
//
////////////////////////////////////////////////////////////////////////////////

using Tofunaut.SharpUnity;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class GameManager
    {
        public readonly GameCamera gameCamera;
        public readonly SharpLight sun;

        // --------------------------------------------------------------------------------------------
        public GameManager()
        {
            gameCamera = GameCamera.Create();
            gameCamera.Render(AppManager.Transform);

            sun = SharpLight.Sun();
            sun.Render(AppManager.Transform);
        }

        // --------------------------------------------------------------------------------------------
        public void CleanUp()
        {
            gameCamera.Destroy();
            sun.Destroy();
        }
    }
}
