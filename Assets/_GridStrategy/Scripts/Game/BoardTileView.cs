////////////////////////////////////////////////////////////////////////////////
//
//  BoardTileView (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 12/11/2019
//
////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class BoardTileView : MonoBehaviour
    {
        private static Dictionary<BoardTile, BoardTileView> _boardTileToView = new Dictionary<BoardTile, BoardTileView>();

        public const float Size = 5f;

        public delegate void InstantiateDelegate(BoardTileView view);

        public BoardTile BoardTile { get; private set; }

        public MeshRenderer meshRenderer;
        public Material evenMaterial;
        public Material oddMaterial;

        private void OnDestroy()
        {
            _boardTileToView.Remove(BoardTile);
        }

        // --------------------------------------------------------------------------------------------
        private void SetMaterial()
        {
            Material toUse;
            if (BoardTile.yCoord % 2 == 0)
            {
                if (BoardTile.xCoord % 2 == 0)
                {
                    toUse = evenMaterial;
                }
                else
                {
                    toUse = oddMaterial;
                }
            }
            else
            {
                if (BoardTile.xCoord % 2 == 0)
                {
                    toUse = oddMaterial;
                }
                else
                {
                    toUse = evenMaterial;
                }
            }

            Material[] sharedMaterials = meshRenderer.sharedMaterials;
            sharedMaterials[0] = Instantiate(toUse);
            meshRenderer.sharedMaterials = sharedMaterials;
        }

        // --------------------------------------------------------------------------------------------
        public static void Create(BoardTile boardTile, InstantiateDelegate callback)
        {
            AppManager.AssetManager.Load(AssetPaths.Prefabs.BoardTileView, (bool successful, GameObject payload) =>
            {
                if (successful)
                {
                    GameObject viewGo = Instantiate(payload, boardTile.Transform, false);

                    BoardTileView view = viewGo.GetComponent<BoardTileView>();
                    view.BoardTile = boardTile;
                    view.SetMaterial();

                    _boardTileToView.Add(boardTile, view);

                    callback(view);
                }
            });
        }

        // --------------------------------------------------------------------------------------------
        public static bool TryGetView(BoardTile boardTile, out BoardTileView view)
        {
            return _boardTileToView.TryGetValue(boardTile, out view);
        }
    }
}