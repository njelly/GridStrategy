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
        public enum EHighlight
        {
            None = 0,
            Move = 1,
            Attack = 2,
            Neutral = 3,
            Heal = 4,
            Spawn = 5,
        }

        private static Dictionary<BoardTile, BoardTileView> _boardTileToView = new Dictionary<BoardTile, BoardTileView>();

        public const float Size = 5f;

        public delegate void InstantiateDelegate(BoardTileView view);

        public BoardTile BoardTile { get; private set; }

        public MeshRenderer meshRenderer;
        public Material evenMaterial;
        public Material oddMaterial;

        [Header("Highlight")]
        public MeshRenderer highlightMeshRenderer;
        public Material highlightMoveMaterial;
        public Material highlightAttackMaterial;
        public Material highlightHealMaterial;
        public Material highlightNeutralMaterial;

        private EHighlight _highlight;

        // --------------------------------------------------------------------------------------------
        private void OnDestroy()
        {
            _boardTileToView.Remove(BoardTile);
        }

        // --------------------------------------------------------------------------------------------
        public void SetHighlight(EHighlight highlight)
        {
            _highlight = highlight;
            SetMaterial();
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

            // take care of highlight
            Material[] highlightSharedMaterials = highlightMeshRenderer.sharedMaterials;
            switch(_highlight)
            {
                case EHighlight.None:
                    highlightMeshRenderer.gameObject.SetActive(false);
                    break;
                case EHighlight.Spawn: // TODO: a different material for spawn than for move?
                case EHighlight.Move:
                    highlightMeshRenderer.gameObject.SetActive(true);
                    highlightSharedMaterials[0] = Instantiate(highlightMoveMaterial);
                    break;
                case EHighlight.Attack:
                    highlightMeshRenderer.gameObject.SetActive(true);
                    highlightSharedMaterials[0] = Instantiate(highlightAttackMaterial);
                    break;
                case EHighlight.Heal:
                    highlightMeshRenderer.gameObject.SetActive(true);
                    highlightSharedMaterials[0] = Instantiate(highlightHealMaterial);
                    break;
                case EHighlight.Neutral:
                    highlightMeshRenderer.gameObject.SetActive(true);
                    highlightSharedMaterials[0] = Instantiate(highlightNeutralMaterial);
                    break;
            }
            highlightMeshRenderer.sharedMaterials = highlightSharedMaterials;
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
                    view._highlight = EHighlight.None;

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