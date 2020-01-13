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
        protected Vector2 Size => new Vector2(500, 200);

        protected readonly Player _player;

        protected SharpUIImage _background;
        protected SharpUIMask _headBackground;
        protected SharpUIImage _headSprite;
        protected SharpUIProgressBar _heroHealthBar;
        protected UIEnergyMeter _energyMeter;

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

            _energyMeter = new UIEnergyMeter($"{_player.name}_energy_meter");
            _energyMeter.SetEnergy(_player.Energy, _player.EnergyCap);
            _energyMeter.alignment = EAlignment.TopLeft;
            _energyMeter.margin = new RectOffset((int)Size.y, 0, 60, 0);
            _background.AddChild(_energyMeter);

            return _background;
        }

        // --------------------------------------------------------------------------------------------
        public void SetEnergy(int currentEnergy, int maxEnergy)
        {
            _energyMeter.SetEnergy(currentEnergy, maxEnergy);
        }

        // --------------------------------------------------------------------------------------------
        public class UIEnergyMeter : SharpUIHorizontalLayout
        {
            private int _maxEnergy;
            private int _currentEnergy;
            private List<UIEnergyIncrement> _energyIncrements;

            // --------------------------------------------------------------------------------------------
            public UIEnergyMeter(string name) : base(name)
            {
                SetFixedSize(0, 0); // no size
                spacing = 10;
                _energyIncrements = new List<UIEnergyIncrement>();
            }

            // --------------------------------------------------------------------------------------------
            public void SetEnergy(int currentEnergy, int maxEnergy)
            {
                _currentEnergy = currentEnergy;
                _maxEnergy = maxEnergy;

                while (_energyIncrements.Count > _maxEnergy)
                {
                    RemoveChild(_energyIncrements[_energyIncrements.Count - 1]);
                    _energyIncrements.RemoveAt(_energyIncrements.Count - 1);
                }

                while (_energyIncrements.Count < _maxEnergy)
                {
                    UIEnergyIncrement newIncrement = new UIEnergyIncrement();
                    AddChild(newIncrement, false);
                    _energyIncrements.Add(newIncrement);
                }

                for(int i = 0; i < _energyIncrements.Count; i++)
                {
                    _energyIncrements[i].IsAvailable = i < _currentEnergy;
                }
            }

            // --------------------------------------------------------------------------------------------
            protected class UIEnergyIncrement : SharpUIImage
            {
                public Vector2 Size => new Vector2(30, 40);

                private Color InactiveColor => new Color32(0x0c, 0x7c, 0x90, 0xff);
                private Color ActiveColor => new Color32(0x79, 0xe7, 0xfb, 0xff);

                public bool IsAvailable
                {
                    get { return _isAvailable; }
                    set
                    {
                        _isAvailable = value;
                        if(IsBuilt)
                        {
                            UpdateVisuals();
                        }
                    }
                }

                private bool _isAvailable;

                // --------------------------------------------------------------------------------------------
                public UIEnergyIncrement() : base("UIEnergyIncrement", null)
                {
                    SetFixedSize(Size);
                    _isAvailable = false;
                }

                // --------------------------------------------------------------------------------------------
                protected override void PostRender()
                {
                    base.PostRender();

                    UpdateVisuals();
                }

                // --------------------------------------------------------------------------------------------
                private void UpdateVisuals()
                {
                    if(_isAvailable)
                    {
                        Color = ActiveColor;
                    }
                    else
                    {
                        Color = InactiveColor;
                    }
                }
            }
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

            _energyMeter.alignment = EAlignment.TopRight;
            _energyMeter.margin = new RectOffset(0, (int)Size.y, 60, 0);

            return toReturn;
        }
    }
}