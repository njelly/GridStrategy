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

        public bool HasMoved { get; private set; }
        public bool HasDoneAction { get; private set; }

        public readonly int id;

        private UnitView _view;
        private UnitData _data;

        private EFacing _facing;
        private TofuAnimation _facingAnim;
        private TofuAnimation _moveAnim;

        // --------------------------------------------------------------------------------------------
        public Unit(UnitData data, BoardTile boardTile) : base(data.id) 
        {
            id = _idCounter++;
            _idToUnit.Add(this);

            _data = data;

            Health = _data.health;
            MoveRange = _data.moveRange;

            MoveTo(boardTile, false);
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
        public void MoveTo(BoardTile boardTile, bool animate)
        {
            // leave the current tile, if it exists
            boardTile?.RemoveOccupant(this);

            // set the new tile, then add this unit as an occupant
            BoardTile = boardTile;
            boardTile.AddOccupant(this);

            if(animate)
            {
                _moveAnim?.Stop();
                throw new NotImplementedException("hey animate the unit to the board tile somehow");
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
