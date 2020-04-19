////////////////////////////////////////////////////////////////////////////////
//
//  BoardTile (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 12/11/2019
//
////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using Tofunaut.Core;
using Tofunaut.SharpUnity;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class BoardTile : SharpGameObject
    {
        public Unit Occupant { get; private set; }
        public IntVector2 Coord { get { return new IntVector2(xCoord, yCoord); } }

        public readonly int xCoord;
        public readonly int yCoord;

        private BoardTileView _view;

        // --------------------------------------------------------------------------------------------
        public BoardTile(int xCoord, int yCoord) : base($"BoardTile {xCoord},{yCoord}")
        {
            this.xCoord = xCoord;
            this.yCoord = yCoord;

            LocalPosition = new Vector3(xCoord * BoardTileView.Size, 0, yCoord * BoardTileView.Size);
        }

        // --------------------------------------------------------------------------------------------
        public void SetOccupant(Unit unit)
        {
            if(Occupant == unit)
            {
                return;
            }

            if(unit != null && Occupant != null)
            {
                Debug.LogError($"the board tile {xCoord}, {yCoord} is already occupied by unit {Occupant.id}");
            }

            Occupant = unit;
        }

        // --------------------------------------------------------------------------------------------
        protected override void Build()
        {
            BoardTileView.Create(this, (BoardTileView view) =>
            {
                _view = view;
            });
        }

        // --------------------------------------------------------------------------------------------
        public int GetMoveCostForUnit(Unit unit)
        {
            // TODO: depending on modifiers on the unit or tile, maybe return a different number?

            if(unit.BoardTile.Coord.Equals(Coord))
            {
                // no cost to move out of the tile the unit is currently on -- should this always be the case?
                return 0;
            }
            else
            {
                return 1;
            }
        }

        // --------------------------------------------------------------------------------------------
        public bool IsAdjacentTo(BoardTile other)
        {
            return (Coord - other.Coord).ManhattanDistance == 1;
        }
    }
}
