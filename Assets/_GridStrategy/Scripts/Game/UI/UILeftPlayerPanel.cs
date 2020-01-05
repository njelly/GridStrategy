using System;
using System.Collections.Generic;
using Tofunaut.GridStrategy.UI;
using Tofunaut.SharpUnity.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Tofunaut.GridStrategy.Game.UI
{
    // --------------------------------------------------------------------------------------------
    public class UILeftPlayerPanel : GridStrategyUIView
    {
        private Vector2 Size => new Vector2(500, 200);

        protected readonly Player _player;

        protected SharpUIImage _background;
        protected SharpUIMask _headBackground;
        protected SharpUIImage _headSprite;
        protected SharpUIProgressBar _heroHealthBar;

        // --------------------------------------------------------------------------------------------
        // always render on top so this blocks input
        public UILeftPlayerPanel(Player player) : base (UIPriorities.UIWorldInteractionManager + 1)
        {
            _player = player;
        }

        // --------------------------------------------------------------------------------------------
        protected override SharpUIBase BuildMainPanel()
        {
            _background = new SharpUIImage($"{_player.name}_Panel", null);
            _background.Color = Color.clear;
            _background.SetFixedSize(Size);
            _background.alignment = EAlignment.BottomLeft;
            _background.margin = new RectOffset(20, 0, 0, 20);

            _heroHealthBar = new SharpUIProgressBar("HeroHealthBar", null, null);
            _heroHealthBar.BackgroundColor = new Color(0f, 0f, 0f, 0.5f);
            _heroHealthBar.FillColor = Color.green;
            _heroHealthBar.Percent = 1f;
            _heroHealthBar.SetFixedSize(340, 40);
            _heroHealthBar.alignment = EAlignment.TopRight;
            _heroHealthBar.margin = new RectOffset(0, 0, 20, 0);
            _background.AddChild(_heroHealthBar);

            _headBackground = new SharpUIMask("HeadBackground", AppManager.AssetManager.Get<Sprite>(AssetPaths.Sprites.CircleWhite2048));
            _headBackground.ShowMaskGraphic = true;
            _headBackground.SetFixedSize((int)Size.y, (int)Size.y);
            _background.AddChild(_headBackground);

            _headSprite = new SharpUIImage("HeadSprite", AppManager.AssetManager.Get<Sprite>(_player.PlayerData.headSpritePath));
            _headSprite.SetFillSize();
            _headBackground.AddChild(_headSprite);

            return _background;
        }
    }

    // --------------------------------------------------------------------------------------------
    public class UIRightPlayerPanel : UILeftPlayerPanel
    {
        public UIRightPlayerPanel(Player player) : base(player) { }

        protected override SharpUIBase BuildMainPanel()
        {
            SharpUIBase toReturn = base.BuildMainPanel();
            _background.alignment = EAlignment.TopRight;
            _background.margin = new RectOffset(0, 20, 20, 0);

            _heroHealthBar.FillDirection = SharpUIProgressBar.EFillDirection.RightToLeft;
            _heroHealthBar.alignment = EAlignment.TopLeft;

            _headBackground.alignment = EAlignment.MiddleRight;

            _headSprite.LocalScale = new Vector3(-1f, 1f, 1f);

            return toReturn;
        }
    }
}