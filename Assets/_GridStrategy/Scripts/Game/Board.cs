////////////////////////////////////////////////////////////////////////////////
//
//  Board (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 12/11/2019
//
////////////////////////////////////////////////////////////////////////////////

using Tofunaut.SharpUnity;
using UnityEngine;

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

        public readonly int width;
        public readonly int height;

        private BoardTile[,] _tiles;

        // --------------------------------------------------------------------------------------------
        public Board(int width, int height) : base("Board")
        {
            this.width = width;
            this.height = height;

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
        public BoardTile GetHeroStartTile(int playerIndex)
        {
            switch(playerIndex)
            {
                case 0:
                    return this[0, 0];
                case 1:
                    return this[width - 1, height - 1];
                default:
                    Debug.LogError($"Unhandled player index for hero start tile: {playerIndex}");
                    return null;
            }
        }

        // --------------------------------------------------------------------------------------------
        protected override void Build() { }
    }
}
