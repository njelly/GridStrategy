using System;
using UnityEngine;

namespace Tofunaut.GridStrategy
{
    [CreateAssetMenu(fileName = "New PlayerData Asset", menuName = "GridStategy/PlayerData Asset")]
    public class PlayerDataAsset : ScriptableObject
    {
        public PlayerData data;
    }

    [Serializable]
    public struct PlayerData
    {
        public string playerName;
    }
}