////////////////////////////////////////////////////////////////////////////////
//
//  Board (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 12/11/2019
//
////////////////////////////////////////////////////////////////////////////////

using Tofunaut.SharpUnity;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class Board : SharpGameObject
    {
        public BoardTile this[int x, int y]
        {
            get
            {
                return _tiles[x, y];
            }
        }

        private BoardTile[,] _tiles;

        // --------------------------------------------------------------------------------------------
        public Board(int width, int height) : base("Board")
        {
            _tiles = new BoardTile[width, height];
            for(int x = 0; x < width; x++)
            {
                for(int y = 0; y < height; y++)
                {
                    BoardTile boardTile = new BoardTile(x, y);
                    AddChild(boardTile);
                    _tiles[x, y] = boardTile;
                }
            }
        }

        // --------------------------------------------------------------------------------------------
        protected override void Build() { }
    }
}
