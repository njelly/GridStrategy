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
<<<<<<< HEAD
    // --------------------------------------------------------------------------------------------
    public class InGameController : ControllerBehaviour
    {

        // --------------------------------------------------------------------------------------------
        private static class State
        {
            public const string Loading = "Loading";
=======
    public class InGameController : ControllerBehaviour
    {
        private static class State
        {
            public const string Loading = "loading";
>>>>>>> 4be22197ba49ddfd51e8a252a38448999337ff54
            public const string InGame = "InGame";
        }

        private TofuStateMachine _stateMachine;

<<<<<<< HEAD
        // --------------------------------------------------------------------------------------------
        private void Awake()
        {
            _stateMachine = new TofuStateMachine();
            _stateMachine.Register(State.Loading, Loading_Enter, null, null);
=======
        private void Awake()
        {
            _stateMachine = new TofuStateMachine();
            _stateMachine.Register(State.Loading, LoadingEnter, null, null);
>>>>>>> 4be22197ba49ddfd51e8a252a38448999337ff54
            _stateMachine.Register(State.InGame, InGame_Enter, null, null);

            _stateMachine.ChangeState(State.Loading);
        }

<<<<<<< HEAD
        // --------------------------------------------------------------------------------------------
        private void Update()
        {
            _stateMachine.Update(Time.deltaTime);
        }

        #region State Machine

        // --------------------------------------------------------------------------------------------
        private void Loading_Enter()
=======
        #region State Machine

        private void LoadingEnter()
>>>>>>> 4be22197ba49ddfd51e8a252a38448999337ff54
        {
            // TODO: actually load stuff

            _stateMachine.ChangeState(State.InGame);
        }

<<<<<<< HEAD
        // --------------------------------------------------------------------------------------------
=======
>>>>>>> 4be22197ba49ddfd51e8a252a38448999337ff54
        private void InGame_Enter()
        {
            Debug.Log("InGame_Enter");
        }

        #endregion State Machine
    }
}
