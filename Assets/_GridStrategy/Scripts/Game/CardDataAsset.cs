////////////////////////////////////////////////////////////////////////////////
//
//  CardDataAsset (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/29/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    [CreateAssetMenu(fileName = "New CardData Asset", menuName = "GridStategy/CardData Asset")]
    public class CardDataAsset : ScriptableObject
    {
        public CardData data;
    }
}