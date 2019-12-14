////////////////////////////////////////////////////////////////////////////////
//
//  GameCamera (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/28/2019
//
////////////////////////////////////////////////////////////////////////////////

using Tofunaut.SharpUnity;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    public class GameCamera : SharpCamera
    {
        protected GameCamera() : base("GameCamera") { }

        protected override void PostRender()
        {
            base.PostRender();

            _unityCamera.clearFlags = CameraClearFlags.SolidColor;
            _unityCamera.backgroundColor = Color.black;
        }

        public static GameCamera Create()
        {
            GameCamera toReturn = new GameCamera
            {
                Tag = "MainCamera",
                LocalPosition = new Vector3(0, 10, -10),
                LocalRotation = Quaternion.Euler(45, 0, 0)
            };

            return toReturn;
        }
    }
}
