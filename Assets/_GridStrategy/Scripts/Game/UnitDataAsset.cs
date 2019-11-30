////////////////////////////////////////////////////////////////////////////////
//
//  UnitDataAsset (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/29/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    [CreateAssetMenu(fileName = "New UnitData Asset", menuName = "GridStategy/UnitData Asset")]
    public class UnitDataAsset : ScriptableObject
    {
        public UnitData data;
    }

    // --------------------------------------------------------------------------------------------
    [Serializable]
    public struct UnitData
    {
        public string name;
        public string prefabPath;
        public float health;
        public int moveSpeed;
    }
}