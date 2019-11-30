////////////////////////////////////////////////////////////////////////////////
//
//  PlayerDataAsset (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/29/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    [CreateAssetMenu(fileName = "New PlayerData Asset", menuName = "GridStategy/PlayerData Asset")]
    public class PlayerDataAsset : ScriptableObject
    {
        public PlayerData data;
    }

    // --------------------------------------------------------------------------------------------
    [Serializable]
    public struct PlayerData
    {
        public string name;
        public UnitDataAsset heroDataAsset;
        public DeckDataAsset deckDataAsset;
    }
}