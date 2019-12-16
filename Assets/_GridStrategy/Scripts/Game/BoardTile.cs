////////////////////////////////////////////////////////////////////////////////
//
//  BoardTile (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 12/11/2019
//
////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using Tofunaut.SharpUnity;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class BoardTile : SharpGameObject
    {
        public IReadOnlyCollection<Unit> Occupants { get { return _occupants.AsReadOnly(); } }

        public readonly int xCoord;
        public readonly int yCoord;

        private BoardTileView _view;
        private List<Unit> _occupants;

        // --------------------------------------------------------------------------------------------
        public BoardTile(int xCoord, int yCoord) : base($"BoardTile {xCoord},{yCoord}")
        {
            this.xCoord = xCoord;
            this.yCoord = yCoord;

            LocalPosition = new Vector3(xCoord * BoardTileView.Size, 0, yCoord * BoardTileView.Size);

            _occupants = new List<Unit>();
        }

        // --------------------------------------------------------------------------------------------
        public void AddOccupant(Unit unit)
        {
            _occupants.Add(unit);
        }

        // --------------------------------------------------------------------------------------------
        public void RemoveOccupant(Unit unit)
        {
            _occupants.Remove(unit);
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
