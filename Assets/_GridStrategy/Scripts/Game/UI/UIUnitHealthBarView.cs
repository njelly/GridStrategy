using Tofunaut.Animation;
using Tofunaut.GridStrategy.Game;
using Tofunaut.SharpUnity.UI;
using Tofunaut.UnityUtils;
using UnityEngine;

namespace Tofunaut.GridStrategy.UI
{
    public class UIUnitHealthBarView : UIGridStrategyView, Updater.IUpdateable
    {
        private static Vector2 Size = new Vector2(120, 10);
        private static Vector2 OffsetFromUnit = new Vector2(0, 100);
        private const float HealthBarAnimTime = 0.5f;

        private readonly Unit _unit;
        private readonly Game.Game _game;

        private SharpUIProgressBar _healthBar;
        private TofuAnimation _healthbarAnim;
        private Vector3 _unitPreviousPos;

        public UIUnitHealthBarView(Game.Game game, Unit unit) : base (UIPriorities.HUD)
        {
            _game = game;
            _unit = unit;

            _unit.OnTookDamage += Unit_OnTookDamage;
        }

        protected override SharpUIBase BuildMainPanel()
        {
            _healthBar = new SharpUIProgressBar("UnitHealthBar", null, null);
            _healthBar.SetFixedSize(Size);
            _healthBar.BackgroundColor = new Color(0f, 0f, 0f, 0.5f);
            _healthBar.FillColor = Color.green;

            return _healthBar;
        }

        public override void Show()
        {
            base.Show();

            UpdatePosition();

            Updater.Instance.Add(this);
        }

        public override void Hide()
        {
            base.Hide();

            Updater.Instance.Remove(this);
        }

        public void Update(float deltaTime)
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (_unit.IsBuilt)
            {
                _unitPreviousPos = _unit.GameObject.transform.position;
            }

            MainPanel.RectTransform.anchoredPosition = OffsetFromUnit + UIMainCanvas.Instance.GetCanvasPositionForWorldPosition(_unitPreviousPos, _game.gameCamera.UnityCamera);
        }

        private void Unit_OnTookDamage(object sender, Unit.DamageEventArgs e)
        {
            if(e.wasKilled)
            {
                _unit.OnTookDamage -= Unit_OnTookDamage;
            }

            if(!IsShowing)
            {
                Show();
            }

            _healthbarAnim?.Stop();

            float startFill = e.previousHealth / (float)e.targetUnit.MaxHealth;
            float endFill = e.newHealth / (float)e.targetUnit.MaxHealth;
            _healthbarAnim = new TofuAnimation()
                .Value01(HealthBarAnimTime, EEaseType.Linear, (float newValue) =>
                {
                    _healthBar.Percent = Mathf.LerpUnclamped(startFill, endFill, newValue);
                })
                .Then()
                .Wait(2f)
                .Then()
                .Execute(() =>
                {
                    Hide();
                })
                .Play();
        }
    }
}