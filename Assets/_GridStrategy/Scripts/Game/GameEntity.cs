////////////////////////////////////////////////////////////////////////////////
//
//  GameEntity (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for WorldZone on 11/28/2019
//
////////////////////////////////////////////////////////////////////////////////

using Tofunaut.SharpUnity;
using Tofunaut.UnityUtils;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public abstract class GameEntity : SharpGameObject
    {
        // --------------------------------------------------------------------------------------------
        public struct Input
        {
            public Vector2 direction;
            public bool interact;
            public bool cancel;
        }

        // --------------------------------------------------------------------------------------------
        protected GameEntity(string name) : base(name) { }

        // --------------------------------------------------------------------------------------------
        public void ProcessInput(Input input)
        {
            HandleDirection(input.direction);
            HandleInteract(input.interact);
            HandleCancel(input.cancel);
        }

        // --------------------------------------------------------------------------------------------
        public abstract void HandleDirection(Vector2 direction);
        public abstract void HandleInteract(bool interact);
        public abstract void HandleCancel(bool cancel);
    }
}
