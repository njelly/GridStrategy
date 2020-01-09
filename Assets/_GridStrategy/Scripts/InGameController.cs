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

        public static Game.Game Game => _instance._game;

        private static InGameController _instance;

        private TofuStateMachine _stateMachine;
        private Game.Game _game;
        private List<PlayerData> _playerDatas;

        // --------------------------------------------------------------------------------------------
        private void Awake()
        {
            if(_instance != null)
            {
                Debug.LogError("Only one instance of InGameController can exist at at a time");
                Destroy(this);
            }

            _instance = this;

            _stateMachine = new TofuStateMachine();
            _stateMachine.Register(State.Loading, Loading_Enter, Loading_Update, null);
            _stateMachine.Register(State.InGame, InGame_Enter, InGame_Update, null);
        }

        private void OnEnable()
        {
            _stateMachine.ChangeState(State.Loading);

            // TODO: eventually this will either need to be synced over the network or somehow decided before the game starts
            _playerDatas = new List<PlayerData>()
            {
                LocalUserManager.LocalUserData.GetPlayerData(),
                AppManager.Config.GetPlayerDataFromOpponentId("first_boss"),
            };
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

        // --------------------------------------------------------------------------------------------
        private void OnDestroy()
        {
            if(_instance == this)
            {
                _instance = null;
            }
        }

        #region State Machine

        // --------------------------------------------------------------------------------------------
        private void Loading_Enter()
        {
            AppManager.AssetManager.Load<Material>(AssetPaths.Materials.WaypointPath);

            // load all necessary assets based on the playerdatas
            foreach (PlayerData playerData in _playerDatas)
            {
                playerData.LoadAssets(AppManager.AssetManager);
            }
        }

        // --------------------------------------------------------------------------------------------
        private void Loading_Update(float deltaTime)
        {
            if (AppManager.AssetManager.Ready)
            {
                _stateMachine.ChangeState(State.InGame);
            }
        }

        // --------------------------------------------------------------------------------------------
        private void InGame_Enter()
        {
            _game = new Game.Game(_playerDatas, 0);
        }

        // --------------------------------------------------------------------------------------------
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
                _fakeDelay = 0f;
            }
        }

        #endregion State Machine

        protected override void Complete(ControllerCompletedEventArgs e)
        {
            base.Complete(e);

            AppManager.AssetManager.Release<Material>(AssetPaths.Materials.WaypointPath);

            // release assets now that we don't need them anymore
            foreach (PlayerData playerData in _playerDatas)
            {
                playerData.ReleaseAssets(AppManager.AssetManager);
            }

            if (_game != null)
            {
                _game.CleanUp();
                _game = null;
            }
        }
    }
}
