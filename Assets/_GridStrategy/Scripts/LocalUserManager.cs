////////////////////////////////////////////////////////////////////////////////
//
//  LocalUserDataManager (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/09/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using Tofunaut.Core;
using Tofunaut.UnityUtils;
using UnityEngine;

namespace Tofunaut.GridStrategy
{
    public struct UserData
    {
        public string name;

        public static UserData DefaultUserData = new UserData()
        {
            name = "Default User",
        };
    }

    // --------------------------------------------------------------------------------------------
    public class LocalUserManager : AutomaticSingletonBehaviour<LocalUserManager>
    {
        public static UserData LocalUserData { get; set; }

        private const string SerializedLocalPlayerDataKey = "local_user_data";

        // --------------------------------------------------------------------------------------------
        protected override void Awake()
        {
            base.Awake();

            name = "LocalUserManager";
            transform.SetParent(AppManager.Transform);

            AccountManager.AuthenticatedSuccessfully += AccountManager_AuthenticatedSuccessfully;
        }

        // --------------------------------------------------------------------------------------------
        protected override void OnDestroy()
        {
            base.OnDestroy();

            AccountManager.AuthenticatedSuccessfully -= AccountManager_AuthenticatedSuccessfully;
        }

        // --------------------------------------------------------------------------------------------
        public void Initialize(Action onComplete)
        {
            string serializedLocalPlayerData = PlayerPrefs.GetString(SerializedLocalPlayerDataKey, string.Empty);
            if(!string.IsNullOrEmpty(serializedLocalPlayerData))
            {
                try
                {
                    LocalUserData = JsonUtility.FromJson<UserData>(serializedLocalPlayerData);
                    onComplete();
                }
                catch
                {
                    PlayerPrefs.SetString(SerializedLocalPlayerDataKey, string.Empty);
                    IntializeWithDefaultData(onComplete);
                }
            }
            else
            {
                IntializeWithDefaultData(onComplete);
            }
        }

        // --------------------------------------------------------------------------------------------
        public void Save(Action onComplete)
        {
            PlayerPrefs.SetString(SerializedLocalPlayerDataKey, JsonUtility.ToJson(LocalUserData));

            // if the user is logged in, attempt to write their data to their account
            if(AccountManager.LoggedIn)
            {
                AccountManager.Instance.WriteUserData(LocalUserData, onComplete, (TofuError errorCode, string errorMessage) =>
                {
                    Debug.LogError($"Failed to write player data remotely, code: {errorCode}, message: {errorMessage}");
                    onComplete();
                });
            }
            else
            {
                onComplete();
            }
        }

        // --------------------------------------------------------------------------------------------
        private void IntializeWithDefaultData(Action onComplete)
        {
            UserData userData = UserData.DefaultUserData;
            onComplete?.Invoke();
        }

        // --------------------------------------------------------------------------------------------
        private void AccountManager_AuthenticatedSuccessfully(object sender, AccountManager.AuthenticatedSuccessfullyEventArgs e)
        {
            UserData copy = LocalUserData;

            string displayName = e.accountData.playerProfileModel.DisplayName;
            if(!string.IsNullOrEmpty(displayName))
            {
                copy.name = displayName;
            }

            LocalUserData = copy;
        }
    }
}