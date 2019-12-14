////////////////////////////////////////////////////////////////////////////////
//
//  Unit (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/28/2019
//
////////////////////////////////////////////////////////////////////////////////

using Tofunaut.Core;
using Tofunaut.SharpUnity;
using Tofunaut.UnityUtils;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class Unit : SharpGameObject
    {
        public static class State
        {
            public const string Move = "move";
            public const string Action = "action";
            public const string Finished = "finished";
            public const string OutOfTurn = "out_of_turn";
        }

        public float Health { get; protected set; }
        public float MoveSpeed { get; protected set; }

        private UnitView _view;
        private UnitData _data;

        // --------------------------------------------------------------------------------------------
        public Unit(UnitData data) : base(data.id) 
        {
            _data = data;

            Health = _data.health;
            MoveSpeed = _data.moveSpeed;
        }

        #region SharpGameObject

        // --------------------------------------------------------------------------------------------
        protected override void Build()
        {
            UnitView.Create(this, _data, (UnitView view) =>
            {
                _view = view;
            });
        }

        #endregion SharpGameObject

        // --------------------------------------------------------------------------------------------
        public virtual void OnPlayerTurnBegan() 
        {
        }

        // --------------------------------------------------------------------------------------------
        public virtual void OnPlayerTurnEnded()
        {
        }
    }
}
