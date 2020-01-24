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
        public interface IListener
        {
            void OnPlayerDraggedOutCard(Card uiCard);
            void OnPlayerReleasedCard(Card uiCard, PointerEventData pointerEventData);
        }

        private const float CardFanAngleStep = 3f;
        private const float CardFanMaxYOffset = 20f;
        private const float CardFanAnimTime = 0.5f;
        private const float CardFanXSpread = 130f;
        private const float CardPeekAmount = 60f;
        private const float CardPeekAnimTime = 0.2f;
        private const float CardCorrectRotAnimTime = 0.2f;
        private const float CardOverPlayableTileAnimTime = 1f;
        private static Vector2 CardOverPlayableTileOffset => new Vector2(200f, 0f);

        private readonly IListener _listener;
        private readonly Game _game;
        private readonly Player _player;
        private readonly Dictionary<Card, UICard> _cardToUICard;
        private readonly Dictionary<UICard, Card> _uiCardToCard;

        private TofuAnimation _cardFanAnim;
        private SharpUIHorizontalLayout _cardLayout;
        private UICard _hoverCard;
        private UICard _draggingCard;
        private Vector2 _dragStartPointerPos;
        private Vector2 _dragStartAnchorPos;
        private Vector2 _draggingCardAnchorPosOffset;
        private TofuAnimation _cardCorrectRotAnim;
        private TofuAnimation _cardOverPlayableTileAnimation;
        private TofuAnimation _cardNotOverPlayableTileAnimation;
        private List<BoardTile> _draggingCardPlayableTiles;

        // --------------------------------------------------------------------------------------------
        public UIPlayerHand(IListener listener, Game game, Player player) : base(UIPriorities.HUD - 1)
        {
            _listener = listener;
            _game = game;
            _player = player;
            _cardToUICard = new Dictionary<Card, UICard>();
            _uiCardToCard = new Dictionary<UICard, Card>();
            _draggingCardPlayableTiles = new List<BoardTile>();
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
                        _uiCardToCard.Add(uiCard, card);
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
                            UICard_OnPointerUp(uiCard, eventArgs.eventData as PointerEventData);
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
        private void UICard_OnPointerDown(UICard uiCard, PointerEventData pointerEventData)
        {
            if(uiCard == _hoverCard)
            {
                _draggingCardPlayableTiles = Card.GetPlayableTiles(_game, _player, uiCard.CardData);
                _draggingCard = _hoverCard;
                _hoverCard = null;
            }
            else
            {
                return;
            }

            _dragStartPointerPos = pointerEventData.position;
            _dragStartAnchorPos = uiCard.RectTransform.anchoredPosition;

            _listener.OnPlayerDraggedOutCard(_uiCardToCard[uiCard]);
        }

        // --------------------------------------------------------------------------------------------
        private void UICard_OnDrag(UICard card, PointerEventData pointerEventData)
        {
            if(card != _draggingCard)
            {
                return;
            }

            Vector2 anchorPos = _dragStartAnchorPos + UIMainCanvas.Instance.PointerPositionToAnchoredPosition(pointerEventData.position - _dragStartPointerPos);

            if(_cardCorrectRotAnim == null)
            {
                Quaternion startRot = card.LocalRotation;
                _cardCorrectRotAnim = new TofuAnimation()
                    .Value01(CardCorrectRotAnimTime, EEaseType.Linear, (float newValue) =>
                    {
                        card.LocalRotation = Quaternion.SlerpUnclamped(startRot, Quaternion.Euler(0f, 0f, 0f), newValue);
                    })
                    .Play();
            }

            // TODO: animate the card to the side when over a tile... not quite working at the moment
            //if(_game.board.RaycastToPlane(pointerEventData.position, out Vector3 worldPos))
            //{
            //    BoardTile boardTile = _game.board.GetBoardTileAtPosition(worldPos);
            //    if(boardTile != null && _draggingCardPlayableTiles.Contains(boardTile))
            //    {
            //        _cardNotOverPlayableTileAnimation?.Stop();
            //        if (_cardOverPlayableTileAnimation == null)
            //        {
            //            _cardOverPlayableTileAnimation = new TofuAnimation()
            //                .Value01(CardOverPlayableTileAnimTime, EEaseType.EaseOutExpo, (float newValue) =>
            //                {
            //                    Vector2 targetOffset = CardOverPlayableTileOffset;
            //                    if(targetOffset.x + _draggingCard.RectTransform.sizeDelta.x > (_draggingCard.Parent as IRectTransformOwner).RectTransform.sizeDelta.x)
            //                    {
            //                        targetOffset.x *= -1;
            //                    }
            //                    _draggingCardAnchorPosOffset = Vector2.LerpUnclamped(Vector2.zero, targetOffset, newValue);
            //                })
            //                .Play();
            //        }
            //    }
            //    else
            //    {
            //        Debug.Log("card NOT over playable tile!");
            //        _cardOverPlayableTileAnimation?.Stop();
            //        if (_cardNotOverPlayableTileAnimation == null)
            //        {
            //            _cardNotOverPlayableTileAnimation = new TofuAnimation()
            //                .Value01(CardOverPlayableTileAnimTime, EEaseType.EaseOutExpo, (float newValue) =>
            //                {
            //                    Vector2 targetOffset = _draggingCardAnchorPosOffset;
            //                    if(_draggingCardAnchorPosOffset.x + _draggingCard.RectTransform.sizeDelta.x > (_draggingCard.Parent as IRectTransformOwner).RectTransform.sizeDelta.x)
            //                    {
            //                        targetOffset.x *= -1;
            //                    }
            //                    _draggingCardAnchorPosOffset = Vector2.LerpUnclamped(targetOffset, Vector2.zero, newValue);
            //                })
            //                .Play();
            //        }
            //    }
            //}
        }

        // --------------------------------------------------------------------------------------------
        private void UICard_OnPointerUp(UICard uiCard, PointerEventData pointerEventData)
        {
            if(uiCard != _draggingCard)
            {
                return;
            }

            _listener.OnPlayerReleasedCard(_uiCardToCard[uiCard], pointerEventData);

            _cardCorrectRotAnim?.Stop();
            _cardCorrectRotAnim = null;

            _cardOverPlayableTileAnimation?.Stop();
            _cardOverPlayableTileAnimation = null;

            _cardNotOverPlayableTileAnimation?.Stop();
            _cardNotOverPlayableTileAnimation = null;

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

                UICard uiCard = _cardToUICard[card];
                _cardToUICard.Remove(card);
                _uiCardToCard.Remove(uiCard);
            }

            PositionCards(true);
        }
    }
}