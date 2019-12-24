////////////////////////////////////////////////////////////////////////////////
//
//  UIWorldInteractionManager (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/28/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Tofunaut.GridStrategy.UI;
using Tofunaut.SharpUnity.UI;
using Tofunaut.SharpUnity.UI.Behaviour;
using Tofunaut.UnityUtils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tofunaut.GridStrategy.Game.UI
{
    // --------------------------------------------------------------------------------------------
    public class UIWorldInteractionManager : SharpUIBase
    {
        private const float SelectUnitTimeLimit = 0.4f;

        // --------------------------------------------------------------------------------------------
        public interface IListener
        {
            void OnSelectedUnitView(UnitView unitView);
        }

        private static UIWorldInteractionManager _instance;

        private readonly Game _game;
        private readonly IListener _listener;

        private UnitView _potentialSelectedUnitView; // the pointer is down, if a raycast hits a unit, we might be selecting it.
        private float _potentialSelectedUnitTime;

        // --------------------------------------------------------------------------------------------
        private UIWorldInteractionManager(IListener listener, Game game) : base("UIWorldIteractionPanel")
        {
            _game = game;
            _listener = listener;

            SetFillSize(EAxis.X, 1f);
            SetFillSize(EAxis.Y, 1f);

            SubscribeToEvent(EEventType.PointerDown, OnPointerDown);
            SubscribeToEvent(EEventType.PointerUp, OnPointerUp);
        }

        // --------------------------------------------------------------------------------------------
        protected override List<Type> GetComponentTypes()
        {
            List<Type> toReturn = base.GetComponentTypes();
            toReturn.Add(typeof(NonDrawingGraphic));

            return toReturn;
        }

        // --------------------------------------------------------------------------------------------
        private void OnPointerDown(object sender, EventSystemEventArgs e)
        {
            // if we have no potential selected unit yet, try to find one
            if (_potentialSelectedUnitView == null)
            {
                PointerEventData pointerEventData = e.eventData as PointerEventData;
                Ray ray = _game.gameCamera.ScreenPointToRay(pointerEventData.position);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    // this could be null
                    _potentialSelectedUnitView = hit.collider.GetComponentInParent<UnitView>();
                    _potentialSelectedUnitTime = Time.time;
                }
            }
        }

        // --------------------------------------------------------------------------------------------
        private void OnPointerUp(object sender, EventSystemEventArgs e)
        {
            if (Time.time - _potentialSelectedUnitTime < SelectUnitTimeLimit && _potentialSelectedUnitView != null)
            {
                // if we have no potential selected unit yet, try to find one
                PointerEventData pointerEventData = e.eventData as PointerEventData;
                Ray ray = _game.gameCamera.ScreenPointToRay(pointerEventData.position);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    // if pointer up is called and we hit same potential selected unit, then we've selected that unit!
                    UnitView view = hit.collider.GetComponentInParent<UnitView>();
                    if (view != null && view == _potentialSelectedUnitView)
                    {
                        _listener.OnSelectedUnitView(view);
                    }
                }
            }

            _potentialSelectedUnitView = null;
        }

        // --------------------------------------------------------------------------------------------
        public static UIWorldInteractionManager Create(IListener listener, Game game)
        {
            if (_instance != null)
            {
                Debug.LogError("An instance of UIWorldIteractionManager already exists!");
                return null;
            }

            _instance = new UIWorldInteractionManager(listener, game);
            UIMainCanvas.Instance.AddChild(_instance, UIPriorities.UIWorldInteractionManager);

            return _instance;
        }
    }
}
