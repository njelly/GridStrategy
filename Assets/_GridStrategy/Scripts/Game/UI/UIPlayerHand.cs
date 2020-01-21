////////////////////////////////////////////////////////////////////////////////
//
//  UIPlayerHand (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for TofuUnity on 01/15/2020
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Tofunaut.Animation;
using Tofunaut.GridStrategy.UI;
using Tofunaut.SharpUnity.UI;
using Tofunaut.SharpUnity.UI.Behaviour;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tofunaut.GridStrategy.Game.UI
{
    // --------------------------------------------------------------------------------------------
    public class UIPlayerHand : UIGridStrategyView
    {
        private const float CardFanAngleStep = 3f;
        private const float CardFanMaxYOffset = 20f;
        private const float CardFanAnimTime = 0.5f;
        private const float CardFanXSpread = 130f;
        private const float CardPeekAmount = 60f;
        private const float CardPeekAnimTime = 0.2f;
        private const float CardCorrectRotAnimTime = 0.2f;

        private readonly Player _player;
        private readonly Dictionary<Card, UICard> _cardToUICard;

        private TofuAnimation _cardFanAnim;
        private SharpUIHorizontalLayout _cardLayout;
        private UICard _hoverCard;
        private UICard _draggingCard;
        private Vector2 _dragStartPointerPos;
        private Vector2 _dragStartAnchorPos;
        private TofuAnimation _cardCorrectRotAnim;

        // --------------------------------------------------------------------------------------------
        public UIPlayerHand(Player player) : base(UIPriorities.HUD - 1)
        {
            _player = player;
            _cardToUICard = new Dictionary<Card, UICard>();
        }

        // --------------------------------------------------------------------------------------------
        public override void Show()
        {
            base.Show();

            _player.Hand.OnPlayerDrewCard += OnPlayerDrewCard;
            _player.Hand.OnPlayerDiscardedCard += OnPlayerDiscardedCard;
        }

        // --------------------------------------------------------------------------------------------
        public override void Hide()
        {
            base.Hide();

            _player.Hand.OnPlayerDrewCard -= OnPlayerDrewCard;
            _player.Hand.OnPlayerDiscardedCard -= OnPlayerDiscardedCard;
        }

        // --------------------------------------------------------------------------------------------
        protected override SharpUIBase BuildMainPanel()
        {
            _cardLayout = new SharpUIHorizontalLayout("UIPlayerHand");
            _cardLayout.SetFixedSize(EAxis.X, _player.Hand.Cards.Count * 200);
            _cardLayout.SetFixedSize(EAxis.Y, 150);
            _cardLayout.spacing = -50;
            _cardLayout.childAlignment = EAlignment.TopCenter;
            _cardLayout.alignment = EAlignment.BottomCenter;

            return _cardLayout;
        }

        // --------------------------------------------------------------------------------------------
        private void PositionCards(bool animate)
        {
            _cardFanAnim?.Stop();

            float[] cardAngles = GetCardFanAngles();
            Vector3[] cardOffsets = GetCardFanOffsets();

            _cardLayout.UpdateChildren();

            int index = 0;
            foreach(Card card in _cardToUICard.Keys)
            {
                UICard uiCard = _cardToUICard[card];

                Quaternion startRotation = uiCard.LocalRotation;
                Quaternion endRotation = Quaternion.Euler(uiCard.LocalRotation.x, uiCard.LocalRotation.y, uiCard.LocalRotation.z - cardAngles[index]);

                Vector3 startPosition = uiCard.LocalPosition;
                Vector3 endPosition = cardOffsets[index];

                index++;

                if(animate)
                {
                    _cardFanAnim = new TofuAnimation()
                        .Value01(CardFanAnimTime, EEaseType.EaseOutExpo, (float newValue) =>
                        {
                            uiCard.LocalRotation = Quaternion.Slerp(startRotation, endRotation, newValue);
                            uiCard.LocalPosition = Vector3.LerpUnclamped(startPosition, endPosition, newValue);
                        })
                        .Play();
                }
                else
                {
                    uiCard.LocalRotation = endRotation;
                    uiCard.LocalPosition = endPosition;
                }
            }
        }

        // --------------------------------------------------------------------------------------------
        private float[] GetCardFanAngles()
        {
            float step = Mathf.Floor(_cardToUICard.Keys.Count / 2f) * CardFanAngleStep * -1f;
            float[] toReturn = new float[_cardToUICard.Keys.Count];

            for(int i = 0; i < toReturn.Length; i++)
            {
                toReturn[i] = step;
                step += CardFanAngleStep;
            }

            return toReturn;
        }

        // --------------------------------------------------------------------------------------------
        private Vector3[] GetCardFanOffsets()
        {
            Vector3[] toReturn = new Vector3[_cardToUICard.Keys.Count];

            float xStep = CardFanXSpread * (_cardToUICard.Keys.Count / -2);

            float angleStep = 180f / _cardToUICard.Keys.Count;
            float angle = 0f;

            for(int i = 0; i < _cardToUICard.Keys.Count; i++)
            {
                toReturn[i] = new Vector3(xStep, Mathf.Sin(angle) * CardFanMaxYOffset);
                angle += angleStep;
                xStep += CardFanXSpread;
            }

            return toReturn;
        }

        // --------------------------------------------------------------------------------------------
        private void OnPlayerDrewCard(object sender, Card.CardEventArgs e)
        {
            int numLoadCalls = 0;
            int numLoadCallsCompleted = 0;

            foreach(Card card in _player.Hand.Cards)
            {
                if(!_cardToUICard.ContainsKey(card))
                {
                    numLoadCalls++;
                    UICard.LoadRequiredAssets(card.cardData, () =>
                    {
                        numLoadCallsCompleted++;

                        UICard uiCard = new UICard(card.cardData);
                        _cardToUICard.Add(card, uiCard);
                        MainPanel.AddChild(uiCard);

                        uiCard.SubscribeToEvent(EEventType.PointerEnter, (object eSender, EventSystemEventArgs eventArgs) =>
                        {
                            UICard_OnPointerEnter(uiCard, eventArgs.eventData as PointerEventData);
                        });
                        uiCard.SubscribeToEvent(EEventType.PointerExit, (object eSender, EventSystemEventArgs eventArgs) =>
                        {
                            UICard_OnPointerExit(uiCard);
                        });
                        uiCard.SubscribeToEvent(EEventType.PointerDown, (object eSender, EventSystemEventArgs eventArgs) =>
                        {
                            UICard_OnPointerDown(uiCard, eventArgs.eventData as PointerEventData);
                        });
                        uiCard.SubscribeToEvent(EEventType.Drag, (object eSender, EventSystemEventArgs eventArgs) =>
                        {
                            UICard_OnDrag(uiCard, eventArgs.eventData as PointerEventData);
                        });
                        uiCard.SubscribeToEvent(EEventType.PointerUp, (object eSender, EventSystemEventArgs eventArgs) =>
                        {
                            UICard_OnPointerUp(uiCard);
                        });

                        if (numLoadCalls == numLoadCallsCompleted)
                        {
                            PositionCards(true);
                        }
                    });
                }
            }
        }

        // --------------------------------------------------------------------------------------------
        private void UICard_OnPointerEnter(UICard uiCard, PointerEventData pointerEventData)
        {
            if(pointerEventData.dragging)
            {
                // ignore when we are dragging from some other UI element
                return;
            }

            int cardIndex = GetIndexForUICard(uiCard);
            float angle = GetCardFanAngles()[cardIndex];
            Vector3 startPos = uiCard.LocalPosition;
            Vector3 endPos = GetCardFanOffsets()[cardIndex] + (Quaternion.Euler(0f, 0f, -angle) * (Vector3.up * CardPeekAmount));

            _hoverCard = uiCard;

            new TofuAnimation()
                .Value01(CardPeekAnimTime, EEaseType.EaseOutExpo, (float newValue) =>
                {
                    uiCard.LocalPosition = Vector3.LerpUnclamped(startPos, endPos, newValue);
                })
                .Play();
        }

        // --------------------------------------------------------------------------------------------
        private void UICard_OnPointerExit(UICard uiCard)
        {
            if(_hoverCard == uiCard)
            {
                _hoverCard = null;
            }

            // only return the card to the hand if we aren't dragging
            if(_draggingCard == null)
            {
                int cardIndex = GetIndexForUICard(uiCard);
                Vector3 startPos = uiCard.LocalPosition;
                Vector3 endPos = GetCardFanOffsets()[cardIndex];

                new TofuAnimation()
                    .Value01(CardPeekAnimTime, EEaseType.EaseOutExpo, (float newValue) =>
                    {
                        uiCard.LocalPosition = Vector3.LerpUnclamped(startPos, endPos, newValue);
                    })
                    .Play();
            }
        }

        // --------------------------------------------------------------------------------------------
        private void UICard_OnPointerDown(UICard card, PointerEventData pointerEventData)
        {
            if(card == _hoverCard)
            {
                _draggingCard = _hoverCard;
                _hoverCard = null;
            }

            _dragStartPointerPos = pointerEventData.position;
            _dragStartAnchorPos = card.RectTransform.anchoredPosition;
        }

        // --------------------------------------------------------------------------------------------
        private void UICard_OnDrag(UICard card, PointerEventData pointerEventData)
        {
            if(card != _draggingCard)
            {
                return;
            }

            card.RectTransform.anchoredPosition = _dragStartAnchorPos + (pointerEventData.position - _dragStartPointerPos);

            if(_cardCorrectRotAnim == null)
            {
                Quaternion startRot = card.LocalRotation;
                _cardCorrectRotAnim = new TofuAnimation()
                    .Value01(CardCorrectRotAnimTime, EEaseType.Linear, (float newValue) =>
                    {
                        card.LocalRotation = Quaternion.SlerpUnclamped(startRot, Quaternion.Euler(0f, 0f, 0f), newValue);
                    })
                    .Then()
                    .Execute(() =>
                    {
                        _cardCorrectRotAnim = null;
                    })
                    .Play();
            }
        }

        // --------------------------------------------------------------------------------------------
        private void UICard_OnPointerUp(UICard card)
        {
            if(card != _draggingCard)
            {
                return;
            }

            _cardCorrectRotAnim?.Stop();
            _cardCorrectRotAnim = null;

            _draggingCard = null;

            PositionCards(true);
        }

        // --------------------------------------------------------------------------------------------
        private int GetIndexForUICard(UICard uiCard)
        {
            int index = 0;
            foreach(Card card in _cardToUICard.Keys)
            {
                if(_cardToUICard[card] == uiCard)
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        // --------------------------------------------------------------------------------------------
        private void OnPlayerDiscardedCard(object sender, Card.CardEventArgs e)
        {
            List<Card> toRemove = new List<Card>();
            foreach(Card card in _cardToUICard.Keys)
            {
                bool isCardInHand = false;
                foreach(Card cardInHand in _player.Hand.Cards)
                {
                    isCardInHand |= cardInHand == card;
                }

                if(!isCardInHand)
                {
                    toRemove.Add(card);
                }
            }

            foreach(Card card in toRemove)
            {
                MainPanel.RemoveChild(_cardToUICard[card], true);
                _cardToUICard.Remove(card);
            }

            PositionCards(true);
        }
    }
}