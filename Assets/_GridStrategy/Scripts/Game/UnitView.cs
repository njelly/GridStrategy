////////////////////////////////////////////////////////////////////////////////
//
//  UnitView (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/29/2019
//
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class UnitView : MonoBehaviour
    {
        public delegate void InstantiateDelegate(UnitView view);

        public Unit Unit { get; private set; }

        public float animMoveSpeed = 10;

        public new Rigidbody rigidbody;
        public new Collider collider;

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

                    callback(view);
                }
            });
        }
    }
}
