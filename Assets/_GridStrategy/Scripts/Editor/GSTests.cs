////////////////////////////////////////////////////////////////////////////////
//
//  GSTests (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/28/2019
//
////////////////////////////////////////////////////////////////////////////////

using Tofunaut.GridStrategy.Game;
using UnityEditor;
using UnityEngine;

namespace Tofunaut.GridStrategy
{
    [InitializeOnLoad]
    public class GSTests
    {
        static GSTests()
        {
            RunTests();
        }

        [MenuItem("GridStrategy/Run Tests")]
        public static void RunTests()
        {
            Unit.RunTests();

            Debug.Log("Tests completed, check console for errors");
        }
    }
}
