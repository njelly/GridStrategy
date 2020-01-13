using Tofunaut.GridStrategy.UI;
using Tofunaut.SharpUnity.UI;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game.UI
{
    // --------------------------------------------------------------------------------------------
    public class UIContextMenuView : UIGridStrategyView
    {
        // --------------------------------------------------------------------------------------------
        public interface IListener
        {
            void OnEndTurnClicked();
        }

        private readonly IListener _listener;
        private UIEndTurnButton _endTurnButton;

        // --------------------------------------------------------------------------------------------
        public UIContextMenuView(IListener listener) : base(UIPriorities.HUD)
        {
            _listener = listener;
        }

        // --------------------------------------------------------------------------------------------
        protected override SharpUIBase BuildMainPanel()
        {
            SharpUIVerticalLayout toReturn = new SharpUIVerticalLayout("UIContextMenuView");
            toReturn.SetFitSize();
            toReturn.spacing = 30;
            toReturn.order = EVerticalOrder.BottomToTop;
            toReturn.alignment = EAlignment.BottomRight;
            toReturn.margin = new RectOffset(0, 30, 0, 30);

            _endTurnButton = new UIEndTurnButton(() =>
            {
                _listener.OnEndTurnClicked();
            });
            toReturn.AddChild(_endTurnButton);

            return toReturn;
        }
    }
}