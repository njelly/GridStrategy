////////////////////////////////////////////////////////////////////////////////
//
//  PlayerDataAsset (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/09/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using Tofunaut.GridStrategy.Game;
using UnityEngine;

namespace Tofunaut.GridStrategy
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
        public string playerName;
        public WorldZone currentWorldZonePrefab;
    }
}