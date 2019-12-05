////////////////////////////////////////////////////////////////////////////////
//
//  UnitDataAsset (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/29/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    [CreateAssetMenu(fileName = "New UnitData Asset", menuName = "GridStategy/UnitData Asset")]
    public class UnitDataAsset : ScriptableObject
    {
        public UnitData data;
    }
}