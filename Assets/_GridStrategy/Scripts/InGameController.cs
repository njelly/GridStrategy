////////////////////////////////////////////////////////////////////////////////
//
//  InGameController (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/26/2019
//
////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using Tofunaut.Core;
using Tofunaut.GridStrategy.Game;
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
        private Game.Game _game;

        // --------------------------------------------------------------------------------------------
        private void Awake()
        {
            _stateMachine = new TofuStateMachine();
            _stateMachine.Register(State.Loading, Loading_Enter, null, null);
            _stateMachine.Register(State.InGame, InGame_Enter, InGame_Update, null);
        }

        private void OnEnable()
        {
            _stateMachine.ChangeState(State.Loading);
        }

        // --------------------------------------------------------------------------------------------
        private void Update()
        {
            _stateMachine.Update(Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                this.Complete(new ControllerCompletedEventArgs(false));
            }
        }

        #region State Machine

        // --------------------------------------------------------------------------------------------
        private void Loading_Enter()
        {
            // TODO: Actually load stuff
            _stateMachine.ChangeState(State.InGame);
        }

        // --------------------------------------------------------------------------------------------
        private void InGame_Enter()
        {
            _game = new Game.Game(new List<PlayerData>
            {
                LocalUserManager.LocalUserData.GetPlayerData(),
                AppManager.Config.GetPlayerDataFromOpponentId("first_boss"),
            }, 0);

            _fakeDelay = 0f;
        }

        private float _fakeDelay;
        private void InGame_Update(float deltaTime)
        {
            if (_game.HasFinished)
            {
                this.Complete(new ControllerCompletedEventArgs(true));
                return;
            }

            if (_game.HasBegun)
            {
                return;
            }

            // TODO: Eventually we want to only call this once all clients have determined they are ready to begin
            _fakeDelay += deltaTime;
            if (_fakeDelay > 1f)
            {
                _game.BeginGame();
            }
        }

        #endregion State Machine

        protected override void Complete(ControllerCompletedEventArgs e)
        {
            base.Complete(e);

            if (_game != null)
            {
                _game.CleanUp();
                _game = null;
            }
        }
    }
}
