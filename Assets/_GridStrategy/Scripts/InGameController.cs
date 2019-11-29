////////////////////////////////////////////////////////////////////////////////
//
//  InGameController (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/26/2019
//
////////////////////////////////////////////////////////////////////////////////

using Tofunaut.Core;
using Tofunaut.UnityUtils;
using UnityEngine;

namespace Tofunaut.GridStrategy
{
    // --------------------------------------------------------------------------------------------
    public class InGameController : ControllerBehaviour
    {
        // --------------------------------------------------------------------------------------------
        private static class State
        {
            public const string Loading = "loading";
            public const string InGame = "InGame";
        }

        private TofuStateMachine _stateMachine;

        // --------------------------------------------------------------------------------------------
        private void Awake()
        {
            _stateMachine = new TofuStateMachine();
            _stateMachine.Register(State.Loading, Loading_Enter, null, null);
            _stateMachine.Register(State.InGame, InGame_Enter, null, null);

            _stateMachine.ChangeState(State.Loading);
        }

        // --------------------------------------------------------------------------------------------
        private void Update()
        {
            _stateMachine.Update(Time.deltaTime);
        }

        #region State Machine

        // --------------------------------------------------------------------------------------------
        private void Loading_Enter()
        {
            // TODO: actually load stuff

            _stateMachine.ChangeState(State.InGame);
        }

        // --------------------------------------------------------------------------------------------
        private void InGame_Enter()
        {
            Debug.Log("InGame_Enter");
        }

        #endregion State Machine
    }
}
