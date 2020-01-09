using System;
using Tofunaut.GridStrategy.UI;
using Tofunaut.SharpUnity.UI;

namespace Tofunaut.GridStrategy.Game.UI
{
    public class UIConfirmationDialogView : GridStrategyUIView
    {
        public Action OnOKClicked;
        public Action OnCancelClicked;

        public UIConfirmationDialogView() : base(UIPriorities.Popup)
        {
            OnOKClicked = () => { };
            OnCancelClicked = () => { };
        }

        protected override SharpUIBase BuildMainPanel()
        {
            
        }
    }
}
