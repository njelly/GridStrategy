////////////////////////////////////////////////////////////////////////////////
//
//  StartMenuController (c) 2019 Tofunaut
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
    public class StartMenuController : ControllerBehaviour, UIStartMenuRootView.IUIStartMenuRootViewListener
    {

        // --------------------------------------------------------------------------------------------
        private static class State
        {
            public const string Loading = "loading";
            public const string Root = "root";
            public const string Disabled = "disabled";
        }

        private TofuStateMachine _stateMachine;
        private StartMenuBackgroundView _backgroundView;
        private UIStartMenuRootView _uiStartMenuRootView;


        // --------------------------------------------------------------------------------------------
        private void Awake()
        {
            _stateMachine = new TofuStateMachine();
            _stateMachine.Register(State.Loading, Loading_Enter, null, Loading_Exit);
            _stateMachine.Register(State.Root, Root_Enter, null, Root_Exit);
            _stateMachine.Register(State.Disabled, null, null, null);
        }


        // --------------------------------------------------------------------------------------------
        private void OnEnable()
        {
            _backgroundView = new StartMenuBackgroundView();
            _uiStartMenuRootView = new UIStartMenuRootView(this);

            _stateMachine.ChangeState(State.Loading);
        }


        // --------------------------------------------------------------------------------------------
        protected override void OnDisable()
        {
            base.OnDisable();

            _backgroundView.Destroy();
            _uiStartMenuRootView.Hide();

            _stateMachine.ChangeState(State.Disabled);
        }


        // --------------------------------------------------------------------------------------------
        private void Update()
        {
            _stateMachine.Update(Time.deltaTime);
        }


        // --------------------------------------------------------------------------------------------
        private void Loading_Enter()
        {
            _backgroundView = new StartMenuBackgroundView();
            new AsyncAssetViewLoader(new[]
            {
                _backgroundView,
            }).LoadAll((float progress) =>
            {
                if(progress.IsApproximately(1f))
                {
                    _stateMachine.ChangeState(State.Root);
                }
            });
        }


        // --------------------------------------------------------------------------------------------
        private void Loading_Exit()
        {
            _backgroundView.Show();
        }


        // --------------------------------------------------------------------------------------------
        private void Root_Enter()
        {
            _uiStartMenuRootView.Show();
        }


        // --------------------------------------------------------------------------------------------
        private void Root_Exit()
        {
            _uiStartMenuRootView.Hide();
        }


        // --------------------------------------------------------------------------------------------
        public void OnRootNewGameClicked()
        {
            Complete(new StartMenuControllerCompletedEventArgs(StartMenuControllerCompletedEventArgs.Intention.StartSinglePlayer, true));
        }


        // --------------------------------------------------------------------------------------------
        public void OnRootContinueClicked()
        {
            Debug.Log("Continue...");
        }


        // --------------------------------------------------------------------------------------------
        public void OnRootMultiplayerClicked()
        {
            Debug.Log("Multiplayer...");
        }


        // --------------------------------------------------------------------------------------------
        public void OnRootOptionsClicked()
        {
            Debug.Log("Options...");
        }
    }

    // --------------------------------------------------------------------------------------------
    public class StartMenuControllerCompletedEventArgs : ControllerCompletedEventArgs
    {
        public enum Intention
        {
            QuitApp,
            StartSinglePlayer,
        }

        public readonly Intention intention;

        public StartMenuControllerCompletedEventArgs(Intention intention, bool successful) : base(successful)
        {
            this.intention = intention;
        }
    }
}