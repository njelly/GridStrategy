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
    public class UIWorldInteractionPanel : SharpUIBase
    {
        private const float SelectUnitTimeLimit = 0.4f;

        // --------------------------------------------------------------------------------------------
        public interface IListener
        {
            void OnSelectedUnitView(UnitView unitView);
            void OnPointerDownOverBoard(BoardTileView boardTileView);
            void OnReleasedBoard(Vector2 releasePosition);
            void OnDragBoard(Vector2 prevDragPosition, Vector2 dragDelta);
            void OnDragFromUnitView(UnitView unitView, Vector2 prevDragPosition, Vector2 dragDelta);
        }

        private static UIWorldInteractionPanel _instance;

        private readonly Game _game;
        private HashSet<IListener> _listeners = new HashSet<IListener>();

        private UnitView _potentialSelectedUnitView; // the pointer is down, if a raycast hits a unit, we might be selecting it.
        private float _potentialSelectedUnitTime;
        private Vector2 _previousDragPoint;

        // --------------------------------------------------------------------------------------------
        private UIWorldInteractionPanel(Game game) : base("UIWorldIteractionPanel")
        {
            _game = game;

            SetFillSize(EAxis.X, 1f);
            SetFillSize(EAxis.Y, 1f);

            SubscribeToEvent(EEventType.PointerDown, OnPointerDown);
            SubscribeToEvent(EEventType.PointerUp, OnPointerUp);
            SubscribeToEvent(EEventType.Drag, OnPointerDrag);
        }

        // --------------------------------------------------------------------------------------------
        protected override List<Type> GetComponentTypes()
        {
            List<Type> toReturn = base.GetComponentTypes();
            toReturn.Add(typeof(NonDrawingGraphic));

            return toReturn;
        }

        // --------------------------------------------------------------------------------------------
        protected override void PostRender()
        {
            base.PostRender();

            if (_instance != null)
            {
                Debug.LogError("An instance of UIWorldIteractionManager already exists!");
                Destroy();
            }
            else
            {
                _instance = this;
            }
        }

        // --------------------------------------------------------------------------------------------
        public override void Destroy()
        {
            base.Destroy();

            if (_instance == this)
            {
                _instance = null;
            }
        }

        // --------------------------------------------------------------------------------------------
        public static void AddListener(IListener listener)
        {
            _instance._listeners.Add(listener);
        }

        // --------------------------------------------------------------------------------------------
        public static void RemoveListener(IListener listener)
        {
            _instance._listeners.Remove(listener);
        }

        // --------------------------------------------------------------------------------------------
        private void OnPointerDown(object sender, EventSystemEventArgs e)
        {
            PointerEventData pointerEventData = e.eventData as PointerEventData;
            Ray ray = _game.gameCamera.ScreenPointToRay(pointerEventData.position);
            if (_potentialSelectedUnitView == null)
            {
                // if we have no potential selected unit yet, try to find one
                if (Physics.Raycast(ray, out RaycastHit unitViewHit))
                {
                    // this could be null
                    _potentialSelectedUnitView = unitViewHit.collider.GetComponentInParent<UnitView>();
                    _potentialSelectedUnitTime = Time.time;
                }

                if (_potentialSelectedUnitView == null)
                {
                    // if the potential slected unit view is still null, lets raycast to our plane
                    _previousDragPoint = pointerEventData.position;
                }
            }

            // see if we hit a board tile view
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                BoardTileView boardTileView = hit.collider.GetComponentInParent<BoardTileView>();
                if (boardTileView != null)
                {
                    foreach (IListener listener in _listeners)
                    {
                        listener.OnPointerDownOverBoard(boardTileView);
                    }
                }
            }
        }

        // --------------------------------------------------------------------------------------------
        private void OnPointerUp(object sender, EventSystemEventArgs e)
        {
            bool upOnUnitView = false;
            PointerEventData pointerEventData = e.eventData as PointerEventData;
            if (Time.time - _potentialSelectedUnitTime < SelectUnitTimeLimit && _potentialSelectedUnitView != null)
            {
                // if we have no potential selected unit yet, try to find one
                Ray ray = _game.gameCamera.ScreenPointToRay(pointerEventData.position);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    // if pointer up is called and we hit same potential selected unit, then we've selected that unit!
                    UnitView view = hit.collider.GetComponentInParent<UnitView>();
                    if (view != null && view == _potentialSelectedUnitView)
                    {
                        foreach(IListener listener in _listeners)
                        {
                            listener.OnSelectedUnitView(view);
                        }
                        upOnUnitView = true;
                    }
                }
            }

            if(!upOnUnitView)
            {
                foreach (IListener listener in _listeners)
                {
                    listener.OnReleasedBoard(pointerEventData.position);
                }
            }

            _potentialSelectedUnitView = null;
        }

        // --------------------------------------------------------------------------------------------
        private void OnPointerDrag(object sender, EventSystemEventArgs e)
        {
            PointerEventData pointerEventData = e.eventData as PointerEventData;
            Vector2 dragDelta = pointerEventData.position - _previousDragPoint;

            if (_potentialSelectedUnitView != null)
            {
                foreach (IListener listener in _listeners)
                {
                    listener.OnDragFromUnitView(_potentialSelectedUnitView, _previousDragPoint, dragDelta);
                }
            }
            else
            {
                foreach (IListener listener in _listeners)
                {
                    listener.OnDragBoard(_previousDragPoint, dragDelta);
                }
            }

            _previousDragPoint = pointerEventData.position;
        }

        // --------------------------------------------------------------------------------------------
        public static UIWorldInteractionPanel Create(Game game)
        {
            if (_instance != null)
            {
                Debug.LogError("An instance of UIWorldIteractionManager already exists!");
                return null;
            }

            UIWorldInteractionPanel toReturn = new UIWorldInteractionPanel(game);
            UIMainCanvas.Instance.AddChild(toReturn, UIPriorities.UIWorldInteractionManager);

            return toReturn;
        }
    }
}
