////////////////////////////////////////////////////////////////////////////////
//
//  UIConfirmationDialogView (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for TofuUnity on 01/09/2020
//
////////////////////////////////////////////////////////////////////////////////

using System;
using Tofunaut.GridStrategy.UI;
using Tofunaut.SharpUnity;
using Tofunaut.SharpUnity.UI;
using Tofunaut.SharpUnity.UI.Behaviour;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game.UI
{
    public class UIConfirmationDialogView : GridStrategyUIView
    {
        private Vector2 Size => new Vector2(500, 300);

        public Action OnOKClicked;
        public Action OnCancelClicked;

        private SharpUIImage _background;

        public UIConfirmationDialogView() : base(UIPriorities.Popup)
        {
            OnOKClicked = () => { };
            OnCancelClicked = () => { };
        }

        protected override SharpUIBase BuildMainPanel()
        {
            // use non drawing graphic to block input
            SharpUINonDrawingGraphic toReturn = new SharpUINonDrawingGraphic("UIConfirmationDialog");
            toReturn.SetFillSize();

            _background = new SharpUIImage($"{toReturn.name}_bg", null);
            _background.SetFixedSize(Size);
            _background.alignment = EAlignment.MiddleCenter;
            _background.Color = new Color(0f, 0f, 0f, 0.5f);

            return toReturn;
        }

        private class ChoiceButton : SharpUIImage
        {
            public Vector2 Size => new Vector2(200, 80);

            private Action _onClick;

            public ChoiceButton(Action onClick, string name, string caption) : base(name, null)
            {
                _onClick = onClick;

                SetFixedSize(Size);
                Color = new Color(0.25f, 0.25f, 0.25f, 1f);

                SharpUITextMeshPro label = new SharpUITextMeshPro($"{name}_label", caption);
                label.SetFillSize();
                label.AutoSizeFont();
                label.Font = AppManager.AssetManager.Get<TMPro.TMP_FontAsset>(AssetPaths.Fonts.GravityBook);
                label.Color = Color.black;
                AddChild(label);

                SubscribeToEvent(EEventType.PointerClick, (object sender, EventSystemEventArgs e))
            }
        }
    }
}
