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
    public class StartMenuController : ControllerBehaviour, UIStartMenuView.IUIStartMenuViewListener
    {

        // --------------------------------------------------------------------------------------------
        private static class State
        {
            public const string Loading = "loading";
            public const string Root = "root";
        }

        private TofuStateMachine _stateMachine;
        private StartMenuBackgroundView _backgroundView;
        private UIStartMenuView _uiStartMenuView;


        // --------------------------------------------------------------------------------------------
        private void Awake()
        {
            _stateMachine = new TofuStateMachine();
            _stateMachine.Register(State.Loading, Loading_Enter, null, Loading_Exit);
            _stateMachine.Register(State.Root, null, null, null);
        }


        // --------------------------------------------------------------------------------------------
        private void OnEnable()
        {
            _backgroundView = new StartMenuBackgroundView();
            _uiStartMenuView = new UIStartMenuView(this);

            _stateMachine.ChangeState(State.Loading);
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
            _uiStartMenuView.Show();
            _backgroundView.Show();
        }


        // --------------------------------------------------------------------------------------------
        public void OnPlayClicked()
        {
            _uiStartMenuView.AnimatePlayButtonSelected(() => { });
        }


        // --------------------------------------------------------------------------------------------
        public void OnLoadoutClicked()
        {
            _uiStartMenuView.AnimateLoadoutButtonSelected(() => { });
        }


        // --------------------------------------------------------------------------------------------
        public void OnSettingsClicked()
        {
            _uiStartMenuView.AnimateSettingsButtonSelected(() => { });
        }
    }
}