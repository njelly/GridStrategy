////////////////////////////////////////////////////////////////////////////////
//
//  UIGameOverView (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for TofuUnity on 01/14/2020
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using Tofunaut.GridStrategy.UI;
using Tofunaut.SharpUnity.UI;
using Tofunaut.SharpUnity.UI.Behaviour;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game.UI
{
    public class UIGameOverView : UIGridStrategyView
    {
        public interface IListener
        {
            void OnReturnToStartClicked();
        }

        private static Vector2 Size => new Vector2(600, 400);

        private readonly IListener _listener;
        private readonly Game _game;

        public UIGameOverView(IListener listener, Game game) : base(UIPriorities.HUD - 1)
        {
            _listener = listener;
            _game = game;
        }

        protected override SharpUIBase BuildMainPanel()
        {
            SharpUINonDrawingGraphic toReturn = new SharpUINonDrawingGraphic("UIGameOverView");
            toReturn.SetFillSize();

            SharpUIImage background = new SharpUIImage($"{toReturn.Name}_bg", null);
            background.Color = new Color(0f, 0f, 0f, 0.5f);
            background.SetFixedSize(Size);
            background.alignment = EAlignment.MiddleCenter;
            toReturn.AddChild(background);

            SharpUITextMeshPro gameOverLabel = new SharpUITextMeshPro("GameOverLabel", "Game Over");
            gameOverLabel.SetFillSize(EAxis.X, 1f);
            gameOverLabel.SetFixedSize(EAxis.Y, 200);
            gameOverLabel.alignment = EAlignment.TopCenter;
            gameOverLabel.AutoSizeFont();
            gameOverLabel.Font = AppManager.AssetManager.Get<TMPro.TMP_FontAsset>(AssetPaths.Fonts.GravityBook);
            gameOverLabel.Color = Color.black;
            gameOverLabel.TextAlignment = TMPro.TextAlignmentOptions.Center;
            gameOverLabel.Color = Color.white;
            background.AddChild(gameOverLabel);

            List<Player> players = _game.GetWinners();
            StringBuilder sb = new StringBuilder();
            if(players.Count > 0)
            {
                sb.Append("Winner: ");
            }
            foreach(Player player in players)
            {
                sb.Append(player.name + ", ");
            }

            SharpUITextMeshPro winnerLabel = new SharpUITextMeshPro("WinnerLabel", sb.ToString());
            winnerLabel.SetFillSize(EAxis.X, 1f);
            winnerLabel.SetFixedSize(EAxis.Y, 100);
            winnerLabel.margin = new RectOffset(0, 0, 200, 0);
            winnerLabel.alignment = EAlignment.TopCenter;
            winnerLabel.AutoSizeFont();
            winnerLabel.Font = AppManager.AssetManager.Get<TMPro.TMP_FontAsset>(AssetPaths.Fonts.GravityItalic);
            winnerLabel.Color = Color.black;
            winnerLabel.TextAlignment = TMPro.TextAlignmentOptions.Center;
            winnerLabel.Color = Color.white;
            background.AddChild(winnerLabel);

            SharpUIHorizontalLayout buttonLayout = new SharpUIHorizontalLayout($"{toReturn.Name}_button_layout");
            buttonLayout.SetFillSize(EAxis.X);
            buttonLayout.SetFixedSize(EAxis.Y, (int)ChoiceButton.Size.y);
            buttonLayout.alignment = EAlignment.BottomCenter;
            buttonLayout.margin = new RectOffset(0, 0, 0, 20);
            buttonLayout.childAlignment = EAlignment.MiddleCenter;
            background.AddChild(buttonLayout);

            buttonLayout.AddChild(new ChoiceButton("ReturnToStartButton", "Return to Start", () =>
            {
                _listener.OnReturnToStartClicked();
            }));

            return toReturn;
        }

        private class ChoiceButton : SharpUIImage
        {
            public static Vector2 Size => new Vector2(200, 80);

            public ChoiceButton(string name, string caption, Action onClick) : base(name, null)
            {
                SetFixedSize(Size);
                Color = new Color(0.25f, 0.25f, 0.25f, 1f);

                SharpUITextMeshPro label = new SharpUITextMeshPro($"{name}_label", caption);
                label.SetFillSize();
                label.AutoSizeFont();
                label.Font = AppManager.AssetManager.Get<TMPro.TMP_FontAsset>(AssetPaths.Fonts.GravityBook);
                label.Color = Color.black;
                label.TextAlignment = TMPro.TextAlignmentOptions.Center;
                label.Color = Color.white;
                AddChild(label);

                SubscribeToEvent(EEventType.PointerClick, (object sender, EventSystemEventArgs e) =>
                {
                    onClick?.Invoke();
                });
            }
        }
    }
}