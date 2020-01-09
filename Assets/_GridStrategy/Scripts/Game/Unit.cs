////////////////////////////////////////////////////////////////////////////////
//
//  Unit (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/28/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using TofuCore;
using Tofunaut.Animation;
using Tofunaut.Core;
using Tofunaut.SharpUnity;
using Tofunaut.UnityUtils;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class Unit : SharpGameObject
    {
        public enum EFacing
        {
            // counter-clockwise
            East = 1,
            North = 2,
            West = 3,
            South = 4,
        }

        private static int _idCounter;
        private static readonly List<Unit> _idToUnit = new List<Unit>();

        public BoardTile BoardTile { get; private set; }
        public float Health { get; protected set; }
        public float MoveRange { get; protected set; }
        public EFacing Facing { get { return _facing; } }
        public bool IsMoving => _moveAnim != null;
        public bool HasMoved { get; private set; }
        public bool HasDoneAction { get; private set; }

        public readonly int id;

        private UnitView _view;
        private UnitData _data;

        private EFacing _facing;
        private TofuAnimation _facingAnim;
        private TofuAnimation _moveAnim;
        private readonly Game _game;

        // --------------------------------------------------------------------------------------------
        public Unit(UnitData data, Game game) : base(data.id) 
        {
            id = _idCounter++;
            _idToUnit.Add(this);

            _data = data;
            _game = game;

            Health = _data.health;
            MoveRange = _data.moveRange;
        }

        #region SharpGameObject

        // --------------------------------------------------------------------------------------------
        protected override void Build()
        {
            UnitView.Create(this, _data, (UnitView view) =>
            {
                _view = view;
            });
        }

        #endregion SharpGameObject

        // --------------------------------------------------------------------------------------------
        public void Move(IntVector2[] path, bool animate, Action onComplete)
        {
            if (animate)
            {
                if(_moveAnim != null)
                {
                    Debug.LogError("move anim in progress!");
                    return;
                }

                _moveAnim = new TofuAnimation();

                Parent.RemoveChild(this, false);

                for (int i = 1; i < path.Length; i++)
                {
                    Vector3 fromPos = _game.board.GetTile(path[i - 1]).Transform.position;
                    Vector3 toPos = _game.board.GetTile(path[i]).Transform.position;
                    float time = (toPos - fromPos).magnitude / _data.travelSpeed;

                    if(i != 1)
                    {
                        // we don't need to call Then() on the first loop
                        _moveAnim.Then();
                    }

                    _moveAnim.Execute(() =>
                    {
                        LocalRotation = Quaternion.LookRotation(toPos - fromPos, Vector3.up);
                    })
                    .Value01(time, EEaseType.Linear, (float newValue) =>
                    {
                        Transform.position = Vector3.LerpUnclamped(fromPos, toPos, newValue);

                        Ray ray = new Ray(Transform.position + Vector3.up, Vector3.down);
                        RaycastHit[] hits = Physics.RaycastAll(ray, 2f);
                        for(int j = 0; j < hits.Length; j++)
                        {
                            BoardTileView boardTileView = hits[j].collider.GetComponentInParent<BoardTileView>();
                            if(boardTileView == null)
                            {
                                continue;
                            }
                            if(boardTileView.BoardTile != BoardTile)
                            {
                                OccupyBoardTile(boardTileView.BoardTile, false);
                            }

                            break;
                        }
                    });
                }

                _moveAnim.Then()
                    .Execute(() =>
                    {
                        OccupyBoardTile(BoardTile, true);
                        _moveAnim = null;

                        onComplete?.Invoke();
                    })
                    .Play();
            }
            else
            {
                for (int i = 0; i < path.Length; i++)
                {
                    BoardTile boardTile = _game.board.GetTile(path[i]);
                    OccupyBoardTile(boardTile, true);
                }

                onComplete?.Invoke();
            }
        }

        // --------------------------------------------------------------------------------------------
        public void OccupyBoardTile(BoardTile boardTile, bool asChild)
        {           
            // do this check so that OccupyBoardTile can be called arbitrarily without re-occupying the same boardtile
            if(BoardTile != boardTile)
            {
                // leave the current tile, if it exists
                boardTile?.RemoveOccupant(this);

                // set the new tile, then add this unit as an occupant
                BoardTile = boardTile;
                boardTile.AddOccupant(this);
            }

            if (asChild)
            {
                // parent the unit to the tile and zero out its local position
                boardTile.AddChild(this);
                LocalPosition = Vector3.zero;
            }
        }

        // --------------------------------------------------------------------------------------------
        public void SetFacing(EFacing facing, bool animate)
        {
            _facing = facing;

            float targetRot = 0f;
            switch(facing)
            {
                case EFacing.North:
                    targetRot = 90;
                    break;
                case EFacing.South:
                    targetRot = 270;
                    break;
                case EFacing.East:
                    targetRot = 0;
                    break;
                case EFacing.West:
                    targetRot = 180;
                    break;
            }

            _facingAnim?.Stop();
            Quaternion endRot = Quaternion.Euler(0f, targetRot, 0f);

            if(animate)
            {
                Quaternion startRot = LocalRotation;
                _facingAnim = new TofuAnimation()
                    .Value01(0.5f, EEaseType.EaseOutExpo, (float newValue) =>
                    {
                        LocalRotation = Quaternion.SlerpUnclamped(startRot, endRot, newValue);
                    })
                    .Play();
            }
            else
            {
                LocalRotation = endRot;
            }
        }

        // --------------------------------------------------------------------------------------------
        public virtual void OnPlayerTurnBegan() 
        {
        }

        // --------------------------------------------------------------------------------------------
        public virtual void OnPlayerTurnEnded()
        {
        }

        // --------------------------------------------------------------------------------------------
        public static Unit GetUnit(int id)
        {
            if(id >= _idToUnit.Count)
            {
                Debug.LogError($"no unit for id {id}");
            }

            return _idToUnit[id];
        }

        // --------------------------------------------------------------------------------------------
        public static EFacing VectorToFacing(Vector2 v)
        {
            float angle = Mathf.Atan2(-v.y, -v.x);
            angle += Mathf.PI;
            angle /= Mathf.PI / 2f;
            int halfQuarter = Convert.ToInt32(angle);
            halfQuarter %= 4;
            return (EFacing)(halfQuarter + 1);
        }
    }
}
