////////////////////////////////////////////////////////////////////////////////
//
//  Unit (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for WorldZone on 11/28/2019
//
////////////////////////////////////////////////////////////////////////////////

using Tofunaut.Core;
using Tofunaut.SharpUnity;
using Tofunaut.UnityUtils;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class Unit : SharpGameObject, Updater.IUpdateable
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
        public string CurrentState { get { return _stateMachine.CurrentState; } }

        private UnitView _unitView;
        private UnitData _data;
        private TofuStateMachine _stateMachine;

        // --------------------------------------------------------------------------------------------
        protected Unit(UnitData data) : base(data.name) 
        {
            _data = data;

            Health = _data.health;
            MoveSpeed = _data.moveSpeed;

            _stateMachine = new TofuStateMachine();
            _stateMachine.Register(State.Move, null, null, null);
            _stateMachine.Register(State.Action, null, null, null);
            _stateMachine.Register(State.Finished, null, null, null);
            _stateMachine.Register(State.OutOfTurn, null, null, null);

            _stateMachine.ChangeState(State.OutOfTurn);
        }

        #region SharpGameObject

        // --------------------------------------------------------------------------------------------
        protected override void Build()
        {
            // Load the UnitView asynchronously
            AppManager.AssetManager.Load(_data.prefabPath, (bool succesfull, GameObject payload) =>
            {
                if(!succesfull)
                {
                    Debug.LogError($"Failed to load the UnitView prefab for {_data.name} at the path {_data.prefabPath}");
                    return;
                }

                _unitView = Object.Instantiate(payload, Transform, false).GetComponent<UnitView>();
                if(_unitView == null)
                {
                    Debug.LogError($"Failed to get the UnitView MonoBehaviour on instantiated prefab for {_data.name} at the path {_data.prefabPath}");
                    return;
                }
            });
        }

        // --------------------------------------------------------------------------------------------
        protected override void PostRender()
        {
            base.PostRender();

            Updater.Instance.Add(this);
        }

        // --------------------------------------------------------------------------------------------
        public override void Destroy()
        {
            base.Destroy();

            Updater.Instance.Remove(this);
        }

        #endregion SharpGameObject

        // --------------------------------------------------------------------------------------------
        public virtual void OnPlayerTurnBegan() 
        {
            if(CurrentState != State.Move)
            {
                _stateMachine.ChangeState(State.Move);
            }
        }

        // --------------------------------------------------------------------------------------------
        public virtual void OnPlayerTurnEnded()
        {
            if (CurrentState != State.OutOfTurn)
            {
                _stateMachine.ChangeState(State.OutOfTurn);
            }
        }

        // --------------------------------------------------------------------------------------------
        public static Unit Create(UnitData data)
        {
            return new Unit(data);
        }

        public void Update(float deltaTime)
        {
            _stateMachine.Update(deltaTime);
        }
    }
}
