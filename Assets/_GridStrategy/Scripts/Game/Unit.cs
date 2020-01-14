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
            // DO NOT RE-ORDER
            // counter-clockwise
            South = 1,
            West = 2,
            North = 3,
            East = 4,
        }

        private static int _idCounter;
        private static readonly List<Unit> _idToUnit = new List<Unit>();

        public BoardTile BoardTile { get; private set; }
        public float Health { get; protected set; }
        public float MoveRange { get; protected set; }
        public EFacing Facing { get { return _facing; } }
        public bool IsMoving => _moveAnim != null;
        public bool HasMoved { get; private set; }
        public bool HasUsedSkill { get; private set; }
        public Player Owner => _owner;

        public readonly int id;

        private UnitView _view;
        private UnitData _data;
        private Player _owner;

        private EFacing _facing;
        private TofuAnimation _facingAnim;
        private TofuAnimation _moveAnim;
        private Action _onMoveComplete;
        private readonly Game _game;

        // --------------------------------------------------------------------------------------------
        public Unit(UnitData data, Game game, Player owner) : base(data.id) 
        {
            id = _idCounter++;
            _idToUnit.Add(this);

            _data = data;
            _game = game;
            _owner = owner;

            Health = _data.health;
            MoveRange = _data.moveRange;

            Player.PlayerTurnStarted += Player_PlayerTurnStarted;
            Player.PlayerTurnEnded += Player_PlayerTurnEnded;
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

        // --------------------------------------------------------------------------------------------
        public override void Destroy()
        {
            base.Destroy();

            Player.PlayerTurnStarted -= Player_PlayerTurnStarted;
            Player.PlayerTurnEnded -= Player_PlayerTurnEnded;
        }

        #endregion SharpGameObject

        // --------------------------------------------------------------------------------------------
        public void Move(IntVector2[] path, bool animate, Action onComplete)
        {
            if (HasMoved)
            {
                Debug.LogError($"Unit {id} has already moved this turn");
                return;
            }

            _onMoveComplete = () =>
            {
                onComplete?.Invoke();
                _onMoveComplete = null;
            };

            HasMoved = true;

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
                        SetFacing(VectorToFacing(toPos - fromPos), false);
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
                        StopOnTile();
                    })
                    .Play();
            }
            else
            {
                for (int i = 0; i < path.Length; i++)
                {
                    BoardTile boardTile = _game.board.GetTile(path[i]);
                    StopOnTile();
                }

                _onMoveComplete?.Invoke();
            }
        }

        // --------------------------------------------------------------------------------------------
        public void UseSkill(EFacing faceTowards, Action onComplete)
        {
            if (HasUsedSkill)
            {
                Debug.LogError($"Unit {id} has already used its skill this turn");
                return;
            }

            if(faceTowards != Facing)
            {
                SetFacing(faceTowards, false);
            }

            Debug.Log($"unit {id} used skill");

            HasUsedSkill = true;

            onComplete?.Invoke();
        }

        // --------------------------------------------------------------------------------------------
        public void StopOnTile()
        {
            if(_moveAnim != null)
            {
                _moveAnim.Stop();
                _onMoveComplete?.Invoke();
                _moveAnim = null;
            }

            OccupyBoardTile(BoardTile, true);
            LocalPosition = Vector3.zero;
        }

        // --------------------------------------------------------------------------------------------
        public void OccupyBoardTile(BoardTile newTile, bool asChild)
        {           
            // do this check so that OccupyBoardTile can be called arbitrarily without re-occupying the same boardtile
            if(BoardTile != newTile)
            {
                // leave the current tile, if it exists 
                BoardTile?.RemoveOccupant(this);

                // set the new tile, then add this unit as an occupant
                BoardTile = newTile;
                newTile.AddOccupant(this);
            }

            if (asChild)
            {
                // parent the unit to the tile and zero out its local position
                newTile.AddChild(this);
                LocalPosition = Vector3.zero;
            }
        }

        // --------------------------------------------------------------------------------------------
        public void SetFacing(EFacing facing, bool animate)
        {
            _facing = facing;
            _facingAnim?.Stop();

            Quaternion rot = FacingToRotation(facing);

            if(animate)
            {
                Quaternion startRot = LocalRotation;
                _facingAnim = new TofuAnimation()
                    .Value01(0.5f, EEaseType.EaseOutExpo, (float newValue) =>
                    {
                        LocalRotation = Quaternion.SlerpUnclamped(startRot, rot, newValue);
                    })
                    .Play();
            }
            else
            {
                LocalRotation = rot;
            }
        }

        // --------------------------------------------------------------------------------------------
        public bool IsAllyOf(Unit other)
        {
            return other.Owner == _owner;
        }

        // --------------------------------------------------------------------------------------------
        public bool IsEnemyOf(Unit other)
        {
            return other.Owner != _owner;
        }

        // --------------------------------------------------------------------------------------------
        private void Player_PlayerTurnStarted(object sender, Player.PlayerEventArgs e)
        {
            if (e.player != Owner)
            {
                return;
            }

            HasMoved = false;
            HasUsedSkill = false;
        }

        // --------------------------------------------------------------------------------------------
        private void Player_PlayerTurnEnded(object sender, Player.PlayerEventArgs e)
        {
            if (e.player != Owner)
            {
                return;
            }

            // TODO: something when the player turn ends
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
        public static EFacing VectorToFacing(Vector3 v) => VectorToFacing(new Vector2(v.x, v.z));
        public static EFacing VectorToFacing(Vector2 v)
        {
            float angle = Mathf.Atan2(-v.y, -v.x);
            angle += Mathf.PI;
            angle /= Mathf.PI / 2f;
            int halfQuarter = Convert.ToInt32(angle);
            halfQuarter %= 4;
            return (EFacing)(halfQuarter + 1);
        }

        // --------------------------------------------------------------------------------------------
        public static Quaternion FacingToRotation(EFacing facing)
        {
            float rot = 0f;
            switch (facing)
            {
                case EFacing.North:
                    rot = 270;
                    break;
                case EFacing.South:
                    rot = 90;
                    break;
                case EFacing.East:
                    rot = 180;
                    break;
                case EFacing.West:
                    rot = 0;
                    break;
            }

            return Quaternion.Euler(0f, rot, 0f);
        }

        // --------------------------------------------------------------------------------------------
        public static IntVector2 RotateVectorForFacingDir(EFacing facing, IntVector2 v)
        {
            switch (facing)
            {
                case EFacing.East:
                    return v;
                case EFacing.South:
                    return v.Rotate90Clockwise();
                case EFacing.West:
                    return v.Rotate90Clockwise().Rotate90Clockwise();
                case EFacing.North:
                    return v.Rotate90Clockwise().Rotate90Clockwise().Rotate90Clockwise();
                default:
                    Debug.LogError($"Facing not implemented: {facing}, can't rotate");
                    return v;
            }
        }
    }
}
