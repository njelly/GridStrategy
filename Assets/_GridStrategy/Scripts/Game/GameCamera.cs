////////////////////////////////////////////////////////////////////////////////
//
//  GameCamera (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/28/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using Tofunaut.Animation;
using Tofunaut.GridStrategy.Game.UI;
using Tofunaut.SharpUnity;
using Tofunaut.UnityUtils;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class GameCamera : SharpCamera, UIWorldInteractionPanel.IListener
    {
        public Vector3 LookingAt { get; private set; }

        public bool dragEnabled;

        private float _distanceFromLookTarget;
        private float _horizonAngle;
        private float _orbitAngle;
        private Plane _groundPlane;

        // --------------------------------------------------------------------------------------------
        protected GameCamera(Game game) : base("GameCamera")
        {
            UIWorldInteractionPanel.Create(this, game);
            dragEnabled = true;
        }

        public void LookAt (Vector3 lookTarget) => LookAt(lookTarget, false);
        public void LookAt (Vector3 lookTarget, bool doAnimate)
        {
            if(doAnimate)
            {
                throw new NotImplementedException("animate plz");
            }
            else
            {
                LookingAt = lookTarget;
                LocalPosition = LookingAt + CalculateOffsetFromTarget();
            }
        }

        // --------------------------------------------------------------------------------------------
        public Vector3 CalculateOffsetFromTarget()
        {
            return Quaternion.Euler(0, -1 * _orbitAngle, 0) * (Quaternion.Euler(0, 0, _horizonAngle) * (Vector3.right * _distanceFromLookTarget));
        }

        // --------------------------------------------------------------------------------------------
        public static GameCamera Create(Game game, float initOribitAngle, Vector3 initLookAt)
        {
            GameCamera toReturn = new GameCamera(game);
            toReturn._orbitAngle = initOribitAngle;
            toReturn._distanceFromLookTarget = 34f;
            toReturn._horizonAngle = 45f;
            toReturn._groundPlane = new Plane(Vector3.up, initLookAt);

            toReturn.LookingAt = initLookAt;

            toReturn.Tag = "MainCamera";
            toReturn.LocalPosition = toReturn.LookingAt + toReturn.CalculateOffsetFromTarget();
            toReturn.LocalRotation = Quaternion.LookRotation(initLookAt - toReturn.LocalPosition, Vector3.up);
            toReturn.CameraClearFlags = CameraClearFlags.SolidColor;
            toReturn.CameraBackgroundColor = Color.black;
            toReturn.CameraFieldOfView = 30f;

            return toReturn;
        }

        public void OnSelectedUnitView(UnitView unitView)
        {
            //LookAt(unitView.Unit.LocalPosition);
        }

        public void OnDrag(Vector2 prevDragPosition, Vector2 dragDelta)
        {
            if(!dragEnabled)
            {
                return;
            }

            Ray prevRay = _unityCamera.ScreenPointToRay(prevDragPosition);
            if(_groundPlane.Raycast(prevRay, out float prevDistance))
            {
                Ray nextRay = _unityCamera.ScreenPointToRay(prevDragPosition + dragDelta);
                if(_groundPlane.Raycast(nextRay, out float nextDistance))
                {
                    LookAt(LookingAt + (prevRay.GetPoint(prevDistance) - nextRay.GetPoint(nextDistance)));
                }
            }
        }
    }
}
