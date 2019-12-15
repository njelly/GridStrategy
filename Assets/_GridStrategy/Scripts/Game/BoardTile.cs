////////////////////////////////////////////////////////////////////////////////
//
//  BoardTile (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 12/11/2019
//
////////////////////////////////////////////////////////////////////////////////

using Tofunaut.SharpUnity;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class BoardTile : SharpGameObject
    {
        public readonly int xCoord;
        public readonly int yCoord;

        private BoardTileView _view;

        // --------------------------------------------------------------------------------------------
        public BoardTile(int xCoord, int yCoord) : base($"BoardTile {xCoord},{yCoord}")
        {
            this.xCoord = xCoord;
            this.yCoord = yCoord;

            LocalPosition = new Vector3(xCoord * BoardTileView.Size, 0, yCoord * BoardTileView.Size);
            Debug.Log($"xCoord: {xCoord}, yCoord: {yCoord}, LocalPosition: {LocalPosition}");
        }

        // --------------------------------------------------------------------------------------------
        protected override void Build()
        {
            BoardTileView.Create(this, (BoardTileView view) =>
            {
                _view = view;
            });
        }
    }
}
