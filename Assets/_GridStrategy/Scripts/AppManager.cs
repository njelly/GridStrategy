////////////////////////////////////////////////////////////////////////////////
//
//  AppManager (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/09/2019
//
////////////////////////////////////////////////////////////////////////////////

using Tofunaut.Core;
using Tofunaut.UnityUtils;
using UnityEngine;

namespace Tofunaut.GridStrategy
{
    // --------------------------------------------------------------------------------------------
    /// <summary>
    /// Initializes the game and manages general systems for the app
    /// </summary>
    public class AppManager : SingletonBehaviour<AppManager>
    {
        // --------------------------------------------------------------------------------------------
        private static class State
        {
            public const string Initializing = "initializing";
            public const string StartMenu = "start_menu";
            public const string InGame = "in_game";
        }

        public static Version AppVersion { get; private set; }
        public static bool IsClientValid { get; private set; }
        public static AssetManager AssetManager { get { return _instance._assetManager; } }
        public static Transform Transform { get { return _instance.transform; } }

        private TofuStateMachine _stateMachine;
        private AssetManager _assetManager;

        // --------------------------------------------------------------------------------------------
        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);

            AppVersion = new Version(string.Format("{0}{1}{2}", Application.version, Version.Delimeter, BuildNumberUtil.ReadBuildNumber()));
            Debug.Log($"GridStrategy {AppVersion} (c) Tofunaut 2019");

            AccountManager.AuthenticatedSuccessfully += AccountManager_AuthenticatedSuccessfully;

            _stateMachine = new TofuStateMachine();
            _stateMachine.Register(State.Initializing, Initializing_Enter, null, Initializing_Exit);
            _stateMachine.Register(State.StartMenu, StartMenu_Enter, null, StartMenu_Exit);
            _stateMachine.Register(State.InGame, InGame_Enter, null, InGame_Exit);
            _stateMachine.ChangeState(State.Initializing);

            _assetManager = new AssetManager();
        }

        // --------------------------------------------------------------------------------------------
        protected override void OnDestroy()
        {
            base.OnDestroy();

            AccountManager.AuthenticatedSuccessfully -= AccountManager_AuthenticatedSuccessfully;
        }

        // --------------------------------------------------------------------------------------------
        private void Update()
        {
            _stateMachine.Update(Time.deltaTime);
        }

        #region State Machine

        // --------------------------------------------------------------------------------------------
        private void Initializing_Enter()
        {
            InitializationController initializationController = gameObject.RequireComponent<InitializationController>();
            initializationController.Completed += InitializationController_Completed;
            initializationController.enabled = true;
        }

        // --------------------------------------------------------------------------------------------
        private void Initializing_Exit()
        {
            InitializationController initializationController = gameObject.RequireComponent<InitializationController>();
            initializationController.Completed -= InitializationController_Completed;
            initializationController.enabled = false;
        }

        // --------------------------------------------------------------------------------------------
        private void StartMenu_Enter()
        {
            StartMenuController startMenuController = gameObject.RequireComponent<StartMenuController>();
            startMenuController.Completed += StartMenuController_Completed;
            startMenuController.enabled = true;
        }

        // --------------------------------------------------------------------------------------------
        private void StartMenu_Exit()
        {
            StartMenuController startMenuController = gameObject.RequireComponent<StartMenuController>();
            startMenuController.Completed -= StartMenuController_Completed;
            startMenuController.enabled = false;
        }

        // --------------------------------------------------------------------------------------------
        private void InGame_Enter()
        {
            InGameController inGameController = gameObject.RequireComponent<InGameController>();
            inGameController.Completed += InGameController_Completed;
            inGameController.enabled = true;
        }

        // --------------------------------------------------------------------------------------------
        private void InGame_Exit()
        {
            InGameController inGameController = gameObject.RequireComponent<InGameController>();
            inGameController.Completed -= InGameController_Completed;
            inGameController.enabled = false;
        }

        #endregion State Machine

        // --------------------------------------------------------------------------------------------
        private void InitializationController_Completed(object sender, ControllerCompletedEventArgs e)
        {
            _stateMachine.ChangeState(State.StartMenu);
        }

        // --------------------------------------------------------------------------------------------
        private void StartMenuController_Completed(object sender, ControllerCompletedEventArgs e)
        {
            StartMenuControllerCompletedEventArgs startMenuControllerCompletedEventArgs = e as StartMenuControllerCompletedEventArgs;
            switch (startMenuControllerCompletedEventArgs.intention)
            {
                case StartMenuControllerCompletedEventArgs.Intention.QuitApp:
                    Application.Quit();
                    Debug.Log("The app has quit");
                    break;
                case StartMenuControllerCompletedEventArgs.Intention.StartSinglePlayer:
                    _stateMachine.ChangeState(State.InGame);
                    break;
            }
        }

        private void InGameController_Completed(object sender, ControllerCompletedEventArgs e)
        {
            _stateMachine.ChangeState(State.StartMenu);
        }

        // --------------------------------------------------------------------------------------------
        private void AccountManager_AuthenticatedSuccessfully(object sender, AccountManager.AuthenticatedSuccessfullyEventArgs e) 
        {
            if(e.accountData.titleData.TryGetValue("required_version", out string versionString))
            {
                IsClientValid = Version.IsValid(versionString, AppVersion);
            }
            else
            {
                throw new System.Exception($"the key [required_version] wasn't found in the title data");
            }
        }
    }
}