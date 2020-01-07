////////////////////////////////////////////////////////////////////////////////
//
//  BoardTileView (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 12/11/2019
//
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class BoardTileView : MonoBehaviour
    {
        public const float Size = 5f;

        public delegate void InstantiateDelegate(BoardTileView view);

        public BoardTile BoardTile { get { return _boardTile; } }

        public MeshRenderer meshRenderer;
        public Material evenMaterial;
        public Material oddMaterial;

        private BoardTile _boardTile;

        // --------------------------------------------------------------------------------------------
        private void SetMaterial()
        {
            Material toUse;
            if (_boardTile.yCoord % 2 == 0)
            {
                if (_boardTile.xCoord % 2 == 0)
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
                if (_boardTile.xCoord % 2 == 0)
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
                    view._boardTile = boardTile;
                    view.SetMaterial();

                    callback(view);
                }
            });
        }
    }
}