//////////////////////////////////////////////////////////////////////////////
//
//  GridStrategyUIView (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 01/03/2020
//
////////////////////////////////////////////////////////////////////////////////

using Tofunaut.SharpUnity.UI;

namespace Tofunaut.GridStrategy.UI
{
    /// <summary>
    /// Assuming all SharpUIView instances will use UIMainCanvas.Instance as their parent, this removes a bit of boilerplate.
    /// </summary>
    public abstract class GridStrategyUIView : SharpUIView
    {
        protected GridStrategyUIView(int renderPriority = 0) : base(UIMainCanvas.Instance, renderPriority) { }
    }
}