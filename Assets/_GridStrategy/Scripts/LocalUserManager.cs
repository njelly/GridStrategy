////////////////////////////////////////////////////////////////////////////////
//
//  LocalUserDataManager (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/09/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Tofunaut.Core;
using Tofunaut.UnityUtils;
using UnityEngine;

namespace Tofunaut.GridStrategy
{
    [Serializable]
    public struct UserData
    {
        public string name;
        public List<string> heroLibrary;
        public List<string> cardLibrary;
        public List<DeckData> decks;

        public PlayerData GetPlayerData()
        {
            return new PlayerData
            {
                name = name,
                deckData = decks[0],
                heroData = AppManager.Config.GetUnitData(heroLibrary[0]),
                headSpritePath = AssetPaths.Sprites.DefaultHeroHead,
                initialSource = 60, // TODO: how should this value be determined?
            };
        }

        public static UserData DefaultUserData = new UserData
        {
            name = "Default User",
            heroLibrary = new List<string> { "default_hero" },
            cardLibrary = new List<string>(AppManager.Config.GetDeckData("tutorial").cardIdToCount.Keys),
            decks = new List<DeckData> { AppManager.Config.GetDeckData("tutorial") },
        };
    }

    // --------------------------------------------------------------------------------------------
    public class LocalUserManager : AutomaticSingletonBehaviour<LocalUserManager>
    {
        public static UserData LocalUserData { get; set; }

        private const string SerializedLocalUserDataKey = "local_user_data";

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
            string serializedLocalPlayerData = PlayerPrefs.GetString(SerializedLocalUserDataKey, string.Empty);
            if (!string.IsNullOrEmpty(serializedLocalPlayerData))
            {
                try
                {
                    LocalUserData = Newtonsoft.Json.JsonConvert.DeserializeObject<UserData>(serializedLocalPlayerData);
                    onComplete();
                }
                catch
                {
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
            PlayerPrefs.SetString(SerializedLocalUserDataKey, Newtonsoft.Json.JsonConvert.SerializeObject(LocalUserData));

            // if the user is logged in, attempt to write their data to their account
            if (AccountManager.LoggedIn)
            {
                AccountManager.Instance.WriteUserData(LocalUserData, onComplete, (TofuErrorCode errorCode, string errorMessage) =>
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
            LocalUserData = UserData.DefaultUserData;
            onComplete?.Invoke();
        }

        // --------------------------------------------------------------------------------------------
        private void AccountManager_AuthenticatedSuccessfully(object sender, AccountManager.AuthenticatedSuccessfullyEventArgs e)
        {
            UserData copy = LocalUserData;

            string displayName = e.accountData.playerProfileModel.DisplayName;
            if (!string.IsNullOrEmpty(displayName))
            {
                copy.name = displayName;
            }

            LocalUserData = copy;
        }
    }
}