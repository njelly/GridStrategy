////////////////////////////////////////////////////////////////////////////////
//
//  DeckDataAsset (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/30/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    [CreateAssetMenu(fileName = "New DeckData Asset", menuName = "GridStategy/DeckData Asset")]
    public class DeckDataAsset : ScriptableObject
    {
        public DeckData data;
    }
}