////////////////////////////////////////////////////////////////////////////////
//
//  UIEndTurnButton (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 01/15/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using Tofunaut.SharpUnity.UI;
using Tofunaut.SharpUnity.UI.Behaviour;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game.UI
{
    // --------------------------------------------------------------------------------------------
    public class UIEndTurnButton : SharpUIImage
    {
        private Vector2 Size => new Vector2(300, 80);

        // --------------------------------------------------------------------------------------------
        public UIEndTurnButton(Action onClick) : base("UIEndTurnButton", null)
        {
            Color = new Color(0.5f, 0.5f, 0.5f, 1f);
            SetFixedSize(Size);
            alignment = EAlignment.BottomRight;

            SharpUITextMeshPro label = new SharpUITextMeshPro($"{Name}_label", "End Turn");
            label.SetFillSize();
            label.AutoSizeFont();
            label.Font = AppManager.AssetManager.Get<TMPro.TMP_FontAsset>(AssetPaths.Fonts.GravityBold);
            label.TextAlignment = TMPro.TextAlignmentOptions.Center;
            AddChild(label);

            SubscribeToEvent(EEventType.PointerClick, (object sender, EventSystemEventArgs e) =>
            {
                onClick?.Invoke();
            });
        }
    }
}