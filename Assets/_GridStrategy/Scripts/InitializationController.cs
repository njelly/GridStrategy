////////////////////////////////////////////////////////////////////////////////
//
//  InitializationController (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/09/2019
//
////////////////////////////////////////////////////////////////////////////////

using Tofunaut.Core;
using Tofunaut.GridStrategy.UI;
using Tofunaut.UnityUtils;
using UnityEngine;

namespace Tofunaut.GridStrategy
{
    // --------------------------------------------------------------------------------------------
    public class InitializationController : ControllerBehaviour
    {
        // --------------------------------------------------------------------------------------------
        private static class State
        {
            public const string InitializeLocalPlayer = "initialize_local_player";
            public const string LogIn = "log_in";
            public const string LoadEssentials = "load_essentials";
        }

        private TofuStateMachine _stateMachine;
        private int _canvasFrameCounter;

        // --------------------------------------------------------------------------------------------
        private void Awake()
        {
            _stateMachine = new TofuStateMachine();
            _stateMachine.Register(State.LoadEssentials, LoadEssentials_Enter, LoadEssentials_Update, null);
            _stateMachine.Register(State.LogIn, LogIn_Enter, null, null);
            _stateMachine.Register(State.InitializeLocalPlayer, InitializeLocalPlayer_Enter, null, null);

            _stateMachine.ChangeState(State.LoadEssentials);
        }

        // --------------------------------------------------------------------------------------------
        private void Update()
        {
            _stateMachine.Update(Time.deltaTime);
        }

        #region State Machine

        // --------------------------------------------------------------------------------------------
        private void LoadEssentials_Enter()
        {
            // create the main canvas
            UIMainCanvas.Create();

            // need to count frames so the canvas has enough time to size itself
            _canvasFrameCounter = 0;

            // load fonts necessary for displaying any text on the screen
            AppManager.AssetManager.Load<TMPro.TMP_FontAsset>(AssetPaths.Fonts.GravityBold);
            AppManager.AssetManager.Load<TMPro.TMP_FontAsset>(AssetPaths.Fonts.GravityBoldItalic);
            AppManager.AssetManager.Load<TMPro.TMP_FontAsset>(AssetPaths.Fonts.GravityBook);
            AppManager.AssetManager.Load<TMPro.TMP_FontAsset>(AssetPaths.Fonts.GravityBookItalic);
            AppManager.AssetManager.Load<TMPro.TMP_FontAsset>(AssetPaths.Fonts.GravityItalic);
            AppManager.AssetManager.Load<TMPro.TMP_FontAsset>(AssetPaths.Fonts.GravityLight);
            AppManager.AssetManager.Load<TMPro.TMP_FontAsset>(AssetPaths.Fonts.GravityLightItalic);
            AppManager.AssetManager.Load<TMPro.TMP_FontAsset>(AssetPaths.Fonts.GravityRegular);
            AppManager.AssetManager.Load<TMPro.TMP_FontAsset>(AssetPaths.Fonts.GravityUltraLight);
            AppManager.AssetManager.Load<TMPro.TMP_FontAsset>(AssetPaths.Fonts.GravityUltraLightItalic);

            // load a few more general use sprites
            AppManager.AssetManager.Load<Sprite>(AssetPaths.Sprites.CircleWhite2048);
        }

        // --------------------------------------------------------------------------------------------
        private void LoadEssentials_Update(float deltaTime)
        {
            _canvasFrameCounter++;

            if (_canvasFrameCounter > 2 && AppManager.AssetManager.Ready)
            {
                _stateMachine.ChangeState(State.LogIn);
            }
        }

        // --------------------------------------------------------------------------------------------
        private void LogIn_Enter()
        {
            AccountManager.Instance.LogIn(() =>
            {
                _stateMachine.ChangeState(State.InitializeLocalPlayer);
            }, 
            (TofuError errorCode, string errorMessage) =>
            {
                Debug.LogError($"error code: {errorCode}, {errorMessage}");
                _stateMachine.ChangeState(State.InitializeLocalPlayer);
            });
        }

        // --------------------------------------------------------------------------------------------
        private void InitializeLocalPlayer_Enter()
        {
            LocalUserManager.Instance.Initialize(() =>
            {
                Complete(new ControllerCompletedEventArgs(true));
            });
        }

        #endregion State Machine
    }
}
