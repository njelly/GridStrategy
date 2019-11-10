////////////////////////////////////////////////////////////////////////////////
//
//  LoginManager (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/09/2019
//
////////////////////////////////////////////////////////////////////////////////

using PlayFab;
using System.Collections.Generic;
using Tofunaut.Core;
using Tofunaut.PlayFabUtils;
using Tofunaut.UnityUtils;
using UnityEngine;

namespace Tofunaut.GridStrategy
{
    // --------------------------------------------------------------------------------------------
    public class AccountManager : AutomaticSingletonBehaviour<AccountManager>
    {
        public static bool LoggedIn { get { return PlayFabClientAPI.IsClientLoggedIn(); } }

        public static event System.EventHandler<AuthenticatedSuccessfullyEventArgs> AuthenticatedSuccessfully;

        private PlayFabAccountAuthenticationManager _authenticationManager;
        private PlayFabAccountData _accountData;


        // --------------------------------------------------------------------------------------------
        protected override void Awake()
        {
            base.Awake();

            name = "AccountManager";
            transform.SetParent(AppManager.Transform);
        }


        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Log in to the local player's account.
        /// </summary>
        public void LogIn(AccountAuthenticationManager.AuthenticationSuccessDelegate onSuccess, AccountAuthenticationManager.AuthenticationFailureDelegate onFailure)
        {
            _authenticationManager = new PlayFabAccountAuthenticationManager(
                new List<SessionProvider>
                {
                    new UnityDeviceIdSessionProvider()
                },
                SystemInfo.deviceUniqueIdentifier,
                AppConsts.PlayFab.TitleID);

            _authenticationManager.Authenticate(() =>
            {
                _accountData = _authenticationManager.AccountData;
                AuthenticatedSuccessfully?.Invoke(this, new AuthenticatedSuccessfullyEventArgs(_accountData));
                onSuccess();
            }, onFailure);
        }


        // --------------------------------------------------------------------------------------------
        public void WritePlayerData(PlayerData playerData, System.Action onSuccess, AccountAuthenticationManager.AuthenticationFailureDelegate onFailure)
        {
            // TODO: This is where the player can save their local data to the cloud
            onSuccess();
        }


        // --------------------------------------------------------------------------------------------
        public class AuthenticatedSuccessfullyEventArgs : System.EventArgs
        {
            public readonly PlayFabAccountData accountData;

            public AuthenticatedSuccessfullyEventArgs(PlayFabAccountData accountData)
            {
                this.accountData = accountData;
            }
        }
    }
}