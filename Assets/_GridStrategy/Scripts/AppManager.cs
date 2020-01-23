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

        // --------------------------------------------------------------------------------------------
        public enum EClientState
        {
            /// <summary>
            /// An error occured while validating the client.
            /// </summary>
            ValidationError = 0,

            /// <summary>
            /// The client is offline, and could not be validated.
            /// </summary>
            Offline = 1,

            /// <summary>
            /// The client needs to be updated.
            /// </summary>
            NeedsUpdate = 2,

            /// <summary>
            /// The client has succesfully validated.
            /// </summary>
            Valid = 3,

            /// <summary>
            /// ForceOffline has been set to true.
            /// </summary>
            ForceOffline = 4,
        }

        public static Version AppVersion { get; private set; }
        public static EClientState ClientState { get; private set; }
        public static bool IsClientValid { get { return ClientState == EClientState.Valid; } }
        public static AssetManager AssetManager { get { return _instance._assetManager; } }
        public static Transform Transform { get { return _instance.transform; } }
        public static Config Config { get; private set; }
        public static bool ForceOffline
        {
            get
            {
#if UNITY_EDITOR
                return _instance._forceOffline;
#else
                return false;
#endif
            }
        }

        [Tooltip("The root object for any lights, canvases, etc. used in the editor. Will be destroyed immediately when the game starts.")]
        [SerializeField] private GameObject _testObjectsRoot;

        [Header("Developer Options")]
        [Tooltip("Run the app as if the user has no internet connection.")]
        [SerializeField] private bool _forceOffline;
        [Tooltip("Enter a game with a custom configuration (Not implemented!) imediately after logging in, skipping the start screen.")]
        [SerializeField] private bool _enterTestGame;
        // TODO: it might be useful to have some sort of scriptable object to configure the test game here

        private TofuStateMachine _stateMachine;
        private AssetManager _assetManager;

        // --------------------------------------------------------------------------------------------
        protected override void Awake()
        {
            base.Awake();

            // test objects are for messing around in the scene and should be destroyed immediately
            Destroy(_testObjectsRoot);

            DontDestroyOnLoad(gameObject);

            AppVersion = new Version(string.Format("{0}{1}{2}", Application.version, Version.Delimeter, BuildNumberUtil.ReadBuildNumber()));
            Debug.Log($"GridStrategy {AppVersion} (c) Tofunaut 2020");

            AccountManager.AuthenticatedSuccessfully += AccountManager_AuthenticatedSuccessfully;
            AccountManager.FailedToAuthenticate += AccountManager_FailedToAuthenticate;

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
            AccountManager.FailedToAuthenticate -= AccountManager_FailedToAuthenticate;
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
            if(ForceOffline)
            {
                Config = Config.DefaultConfig();
                ClientState = EClientState.ForceOffline;
            }

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
#if UNITY_EDITOR
            if(_enterTestGame)
            {
                // TODO: somehow configure the test game in the unity editor before entering
                _stateMachine.ChangeState(State.InGame);
                return;
            }
#endif
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
                case StartMenuControllerCompletedEventArgs.Intention.EnterTestGame:
                    _stateMachine.ChangeState(State.InGame);
                    break;
            }
        }

        // --------------------------------------------------------------------------------------------
        private void InGameController_Completed(object sender, ControllerCompletedEventArgs e)
        {
            InGameController.InGameControllerCompletedEventArgs inGameControllerEventArgs = e as InGameController.InGameControllerCompletedEventArgs;
            switch(inGameControllerEventArgs.intention)
            {
                case InGameController.InGameControllerCompletedEventArgs.Intention.ReturnToStart:
                    _stateMachine.ChangeState(State.StartMenu);
                    break;
                default:
                    Debug.LogError($"unhandled ingamecontroller intention: {inGameControllerEventArgs.intention}");
                    break;
            }

            _stateMachine.ChangeState(State.StartMenu);
        }

        // --------------------------------------------------------------------------------------------
        private void AccountManager_AuthenticatedSuccessfully(object sender, AccountManager.AuthenticatedSuccessfullyEventArgs e) 
        {
            Debug.Log("authenticated succesfully");

            if(e.accountData.titleData.TryGetValue("required_version", out string versionString))
            {
                if(Version.IsValid(versionString, AppVersion))
                {
                    ClientState = EClientState.Valid;
                }
                else
                {
                    ClientState = EClientState.NeedsUpdate;
                }
            }
            else
            {
                ClientState = EClientState.ValidationError;
                throw new System.Exception($"the key required_version wasn't found in the title data");
            }

            if(e.accountData.titleData.TryGetValue("game_config", out string serializedConfig))
            {
                serializedConfig = serializedConfig.Replace(@"\r", "");
                serializedConfig = serializedConfig.Replace(@"\n", "");

                Config = new Config(serializedConfig, true);
            }
            else
            {
                Debug.LogError("the key game_config wasn't found in the title data");

                Config = Config.DefaultConfig();
            }
        }

        // --------------------------------------------------------------------------------------------
        private void AccountManager_FailedToAuthenticate(object sender, System.EventArgs e)
        {
            Debug.Log("failed to authenticate, continuing with default data");

            ClientState = EClientState.Offline;
            Config = Config.DefaultConfig();
        }
    }
}
