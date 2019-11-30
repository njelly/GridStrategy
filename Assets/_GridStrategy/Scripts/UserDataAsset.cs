////////////////////////////////////////////////////////////////////////////////
//
//  UserDataAsset (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/09/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Tofunaut.GridStrategy.Game;
using UnityEngine;

namespace Tofunaut.GridStrategy
{

    // --------------------------------------------------------------------------------------------
    [CreateAssetMenu(fileName = "New UserData Asset", menuName = "GridStategy/UserData Asset")]
    public class UserDataAsset : ScriptableObject
    {
        public UserData data;
    }


    // --------------------------------------------------------------------------------------------
    [Serializable]
    public struct UserData
    {
        public string name;
        public List<CardDataAsset> cardLibrary;
        public List<DeckDataAsset> decks;
    }
}