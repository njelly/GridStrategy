using UnityEngine;

namespace Tofunaut.GridStrategy
{
    [CreateAssetMenu(fileName = "New PlayerData Asset", menuName = "GridStategy/PlayerData Asset")]
    public class PlayerDataAsset : ScriptableObject
    {
        public PlayerData data;
    }
}