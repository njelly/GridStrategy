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
using Tofunaut.GridStrategy.UI;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class UnitView : MonoBehaviour
    {
        private static Dictionary<Unit, UnitView> _unitToView = new Dictionary<Unit, UnitView>();

        public delegate void InstantiateDelegate(UnitView view);

        public Unit Unit { get; private set; }

        public new Rigidbody rigidbody;
        public new Collider collider;

        private UIUnitHealthBarView _healthBarView;

        // --------------------------------------------------------------------------------------------
        private void OnDestroy()
        {
            // Unit will be null when this prefab is instantiated without calling Create
            if(Unit != null)
            {
                _unitToView.Remove(Unit);
            }
        }

        // --------------------------------------------------------------------------------------------
        public static void CreateForGame(Game game, Unit unit, UnitData data, InstantiateDelegate callback)
        {
            AppManager.AssetManager.Load(data.prefabPath, (bool successful, GameObject payload) =>
            {
                if (successful)
                {
                    GameObject viewGo = Instantiate(payload, unit.Transform, false);

                    UnitView view = viewGo.GetComponent<UnitView>();
                    view.Unit = unit;
                    view._healthBarView = new UIUnitHealthBarView(game, unit);

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
