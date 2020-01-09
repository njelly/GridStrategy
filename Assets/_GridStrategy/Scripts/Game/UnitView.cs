////////////////////////////////////////////////////////////////////////////////
//
//  UnitView (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/29/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using TofuCore;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class UnitView : MonoBehaviour
    {
        private static Dictionary<Unit, UnitView> _unitToView = new Dictionary<Unit, UnitView>();

        public delegate void InstantiateDelegate(UnitView view);

        public Unit Unit { get; private set; }

        public float animMoveSpeed = 10;

        public new Rigidbody rigidbody;
        public new Collider collider;

        // --------------------------------------------------------------------------------------------
        private void OnDestroy()
        {
            _unitToView.Remove(Unit);
        }

        // --------------------------------------------------------------------------------------------
        public static void Create(Unit unit, UnitData data, InstantiateDelegate callback)
        {
            AppManager.AssetManager.Load(data.prefabPath, (bool successful, GameObject payload) =>
            {
                if (successful)
                {
                    GameObject viewGo = Instantiate(payload, unit.Transform, false);

                    UnitView view = viewGo.GetComponent<UnitView>();
                    view.Unit = unit;

                    _unitToView.Add(unit, view);

                    callback(view);
                }
            });
        }

        // --------------------------------------------------------------------------------------------
        public static bool TryGetView(Unit unit, out UnitView view)
        {
            return _unitToView.TryGetValue(unit, out view);
        }
    }
}
