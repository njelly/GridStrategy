////////////////////////////////////////////////////////////////////////////////
//
//  GameCamera (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/28/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using Tofunaut.GridStrategy.Game.UI;
using Tofunaut.SharpUnity;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class GameCamera : SharpCamera, UIWorldInteractionPanel.IListener
    {
        public Vector3 LookingAt { get; private set; }

        public bool dragEnabled;

        private readonly Game _game;

        private float _distanceFromLookTarget;
        private float _horizonAngle;
        private float _orbitAngle;

        // --------------------------------------------------------------------------------------------
        protected GameCamera(Game game) : base("GameCamera")
        {
            dragEnabled = true;

            _game = game;
        }

        // --------------------------------------------------------------------------------------------
        public override void Destroy()
        {
            base.Destroy();

            UIWorldInteractionPanel.RemoveListener(this);
        }

        // --------------------------------------------------------------------------------------------
        public void LookAt(Vector3 lookTarget) => LookAt(lookTarget, false);
        public void LookAt(Vector3 lookTarget, bool doAnimate)
        {
            if (doAnimate)
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
            return Quaternion.Euler(0, -1 * _orbitAngle, _horizonAngle) * (Vector3.right * _distanceFromLookTarget);
        }

        // --------------------------------------------------------------------------------------------
        public static GameCamera Create(Game game, float initOribitAngle, Vector3 initLookAt)
        {
            GameCamera toReturn = new GameCamera(game);
            toReturn._orbitAngle = initOribitAngle;
            toReturn._distanceFromLookTarget = 34f;
            toReturn._horizonAngle = 45f;

            toReturn.LookingAt = initLookAt;

            toReturn.Tag = "MainCamera";
            toReturn.LocalPosition = toReturn.LookingAt + toReturn.CalculateOffsetFromTarget();
            toReturn.LocalRotation = Quaternion.LookRotation(initLookAt - toReturn.LocalPosition, Vector3.up);
            toReturn.CameraClearFlags = CameraClearFlags.SolidColor;
            toReturn.CameraBackgroundColor = Color.black;
            toReturn.CameraFieldOfView = 30f;

            UIWorldInteractionPanel.AddListener(toReturn);

            return toReturn;
        }

        #region UIWorldInteractionPanel.IListener

        // --------------------------------------------------------------------------------------------
        public void OnSelectedUnitView(UnitView unitView) { }

        // --------------------------------------------------------------------------------------------
        public void OnDragBoard(Vector2 prevDragPosition, Vector2 dragDelta)
        {
            if (!dragEnabled)
            {
                return;
            }



            //if (_groundPlane.Raycast(prevRay, out float prevDistance))
            //{
            //    Ray nextRay = UnityCamera.ScreenPointToRay(prevDragPosition + dragDelta);
            //    if (_groundPlane.Raycast(nextRay, out float nextDistance))
            //    {
            //        LookAt(LookingAt + (prevRay.GetPoint(prevDistance) - nextRay.GetPoint(nextDistance)));
            //    }
            //}
        }

        // --------------------------------------------------------------------------------------------
        public void OnDragFromUnitView(UnitView unitView, Vector2 prevDragPosition, Vector2 dragDelta) { }

        // --------------------------------------------------------------------------------------------
        public void OnReleasedBoard(Vector2 releasePosition) { }

        // --------------------------------------------------------------------------------------------
        public void OnPointerDownOverBoard(BoardTileView boardTileView) { }

        #endregion UIWorldInteractionPanel.IListener
    }
}
