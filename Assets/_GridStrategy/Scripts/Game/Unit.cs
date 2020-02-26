////////////////////////////////////////////////////////////////////////////////
//
//  Unit (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/28/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public event EventHandler<DamageEventArgs> OnTookDamage;

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

        public ReadOnlyCollection<UnitModifier> Modifiers => _modifiers.AsReadOnly();
        public BoardTile BoardTile { get; private set; }
        public Skill Skill { get; private set; }
        public int Health { get; protected set; }
        public int MaxHealth => _data.health;
        public int MoveRange { get; protected set; }
        public EFacing Facing { get { return _facing; } }
        public bool IsMoving => _moveAnim != null;
        public bool HasUsedSkill { get; private set; }
        public bool HasMoved { get; private set; }
        public Player Owner => _owner;
        public bool IsDead => Health <= 0;

        /// <summary>
        /// True when this unit has existed for at least one full turn and hasn't yet used it's skill.
        /// </summary>
        public bool CanUseSkill => _numTurnsActive > 0 && !HasUsedSkill;

        /// <summary>
        /// True when this unit has existed for at least one full turn, hasn't used it's skill, and hasn't moved yet.
        /// </summary>
        public bool CanMove => _numTurnsActive > 0 && !HasUsedSkill && !HasMoved;

        public readonly int id;

        private UnitView _view;
        private UnitData _data;
        private Player _owner;
        private int _numTurnsActive;

        private EFacing _facing;
        private TofuAnimation _facingAnim;
        private TofuAnimation _moveAnim;
        private Action _onMoveComplete;
        private readonly Game _game;
        private readonly List<UnitModifier> _modifiers;
        private UnitModifierTotals _modifierTotals;

        // --------------------------------------------------------------------------------------------
        public Unit(UnitData data, Game game, Player owner) : base(data.id)
        {
            id = _idCounter++;
            _idToUnit.Add(this);

            _data = data;
            _game = game;
            _owner = owner;

            _modifiers = new List<UnitModifier>();
            _modifierTotals = UnitModifierTotals.Identity;

            Health = _data.health;
            MoveRange = _data.moveRange;

            Skill = new Skill(AppManager.Config.GetSkillData(_data.skillId), _game, this);
        }

        #region SharpGameObject

        // --------------------------------------------------------------------------------------------
        protected override void Build()
        {
            UnitView.CreateForGame(_game, this, _data, (UnitView view) =>
            {
                _view = view;
            });

            _owner.PlayerTurnStarted += Player_PlayerTurnStarted;
            _owner.PlayerTurnEnded += Player_PlayerTurnEnded;
        }

        // --------------------------------------------------------------------------------------------
        public override void Destroy()
        {
            base.Destroy();

            _owner.PlayerTurnStarted -= Player_PlayerTurnStarted;
            _owner.PlayerTurnEnded -= Player_PlayerTurnEnded;

            BoardTile?.SetOccupant(null);
            BoardTile = null;
        }

        #endregion SharpGameObject

        // --------------------------------------------------------------------------------------------
        public void Move(IntVector2[] path, bool animate, Action onComplete)
        {
            if (!CanMove)
            {
                Debug.LogError($"Unit {id} cannot move");
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
                if (_moveAnim != null)
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

                    if (i != 1)
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
                        for (int j = 0; j < hits.Length; j++)
                        {
                            BoardTileView boardTileView = hits[j].collider.GetComponentInParent<BoardTileView>();
                            if (boardTileView == null)
                            {
                                continue;
                            }
                            if (boardTileView.BoardTile != BoardTile)
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
        public void UseSkill(EFacing faceTowards, IntVector2 targetCoord, Action onComplete)
        {
            if (!CanUseSkill)
            {
                Debug.LogError($"Unit {id} cannot use its skill");
                return;
            }

            HasUsedSkill = true;

            if (faceTowards != Facing)
            {
                SetFacing(faceTowards, false);
            }

            int numTimesSkillUsedOnTarget = 0;
            int numTimesSkillUsedOnTargetCompleted = 0;
            void SkillUsedOnTargetCallback()
            {
                numTimesSkillUsedOnTargetCompleted++;
                if (numTimesSkillUsedOnTargetCompleted >= numTimesSkillUsedOnTarget)
                {
                    onComplete?.Invoke();
                }
            }

            if (Skill.Target == SkillData.ETarget.None)
            {
                numTimesSkillUsedOnTarget++;
                UseSkillNoTarget(SkillUsedOnTargetCallback);
            }
            else
            {
                List<BoardTile> targetTiles = Skill.GetAffectedTiles(faceTowards, targetCoord);
                foreach (BoardTile boardTile in targetTiles)
                {
                    if (Skill.Target == SkillData.ETarget.Tile)
                    {
                        numTimesSkillUsedOnTarget++;
                        UseSkillOnBoardTile(boardTile, SkillUsedOnTargetCallback);
                    }
                    else
                    {
                        switch (Skill.Target)
                        {
                            case SkillData.ETarget.Ally:
                                if (IsAllyOf(boardTile.Occupant))
                                {
                                    numTimesSkillUsedOnTarget++;
                                    UseSkillOnUnit(boardTile.Occupant, SkillUsedOnTargetCallback);
                                }
                                break;
                            case SkillData.ETarget.Enemy:
                                if (!IsAllyOf(boardTile.Occupant))
                                {
                                    numTimesSkillUsedOnTarget++;
                                    UseSkillOnUnit(boardTile.Occupant, SkillUsedOnTargetCallback);
                                }
                                break;
                            case SkillData.ETarget.Self:
                                if (boardTile.Occupant == this)
                                {
                                    numTimesSkillUsedOnTarget++;
                                    UseSkillOnUnit(boardTile.Occupant, SkillUsedOnTargetCallback);
                                }
                                break;
                        }
                    }
                }
            }
        }

        // --------------------------------------------------------------------------------------------
        private void UseSkillOnUnit(Unit targetUnit, Action onComplete)
        {
            if (Skill.DamageDealt > 0)
            {
                targetUnit.TakeDamage(this, Skill.DamageDealt);
                onComplete?.Invoke();
            }
        }

        // --------------------------------------------------------------------------------------------
        private void UseSkillOnBoardTile(BoardTile boardTile, Action onComplete)
        {
            throw new NotImplementedException();
        }

        // --------------------------------------------------------------------------------------------
        private void UseSkillNoTarget(Action onComplete)
        {
            throw new NotImplementedException();
        }

        // --------------------------------------------------------------------------------------------
        private void TakeDamage(Unit sourceUnit, int amount)
        {
            int previousHealth = Health;
            Health = Mathf.Clamp(Health - amount, 0, Health);

            OnTookDamage?.Invoke(this, new DamageEventArgs(sourceUnit, this, previousHealth, Health, Health <= 0));

            if (Health <= 0)
            {
                Destroy();
            }
        }

        // --------------------------------------------------------------------------------------------
        private void ApplyModifier(UnitModifierData modifierData)
        {
            UnitModifier modifier = new UnitModifier(modifierData, _game, this);
            modifier.OnModifierExpired += Modifier_OnModifierExpired;
            _modifiers.Add(modifier);

            _modifierTotals = UnitModifier.CalculateTotals(_modifiers);
        }

        // --------------------------------------------------------------------------------------------
        private void RemoveModifier(UnitModifier modifier)
        {
            _modifiers.Remove(modifier);
            modifier.OnModifierExpired -= Modifier_OnModifierExpired;
            _modifierTotals = UnitModifier.CalculateTotals(_modifiers);
        }

        // --------------------------------------------------------------------------------------------
        private void Modifier_OnModifierExpired(object sender, UnitModifier.ModifierEventArgs e)
        {
            RemoveModifier(e.modifier);
        }

        // --------------------------------------------------------------------------------------------
        public void StopOnTile()
        {
            if (_moveAnim != null)
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
            if (BoardTile != newTile)
            {
                // leave the current tile, if it exists 
                BoardTile?.SetOccupant(null);

                // set the new tile, then add this unit as an occupant
                BoardTile = newTile;
                newTile.SetOccupant(this);
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

            if (animate)
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
            if (other == null)
            {
                return false;
            }

            return other.Owner == _owner;
        }

        // --------------------------------------------------------------------------------------------
        public bool IsEnemyOf(Unit other)
        {
            if (other == null)
            {
                return false;
            }

            return other.Owner != _owner;
        }

        // --------------------------------------------------------------------------------------------
        private void Player_PlayerTurnStarted(object sender, Player.PlayerEventArgs e)
        {
            _numTurnsActive++;
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
        public static bool CanSpawnOnTile(UnitData unitData, BoardTile boardTile, Unit spawnFrom)
        {
            if (boardTile.Occupant != null)
            {
                return false;
            }

            if ((boardTile.Coord - spawnFrom.BoardTile.Coord).ManhattanDistance > 1)
            {
                return false;
            }

            return true;
        }

        // --------------------------------------------------------------------------------------------
        public static Unit GetUnit(int id)
        {
            if (id >= _idToUnit.Count)
            {
                Debug.LogError($"no unit for id {id}");
            }

            return _idToUnit[id];
        }

        // --------------------------------------------------------------------------------------------
        public static EFacing VectorToFacing(Vector2 v) => VectorToFacing(new Vector3(v.x, 0f, v.y));
        public static EFacing VectorToFacing(Vector3 v)
        {
            float highestValue = float.MinValue;
            EFacing mostAligned = 0;
            foreach (EFacing facingEnum in Enum.GetValues(typeof(EFacing)))
            {
                float dot = Vector3.Dot(FacingToRotation(facingEnum) * Vector2.right, v);
                if (dot > highestValue)
                {
                    highestValue = dot;
                    mostAligned = facingEnum;
                }
            }
            return mostAligned;
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

        // --------------------------------------------------------------------------------------------
        /// <summary>
        /// Unit tests for Unit.cs
        /// </summary>
        public static void RunTests()
        {
            Debug.Assert(VectorToFacing(FacingToRotation(EFacing.North) * Vector3.right) == EFacing.North);
            Debug.Assert(VectorToFacing(FacingToRotation(EFacing.South) * Vector3.right) == EFacing.South);
            Debug.Assert(VectorToFacing(FacingToRotation(EFacing.East) * Vector3.right) == EFacing.East);
            Debug.Assert(VectorToFacing(FacingToRotation(EFacing.West) * Vector3.right) == EFacing.West);
        }

        // --------------------------------------------------------------------------------------------
        public class DamageEventArgs : EventArgs
        {
            public readonly Unit sourceUnit;
            public readonly Unit targetUnit;
            public readonly int previousHealth;
            public readonly int newHealth;
            public readonly bool wasKilled;

            public DamageEventArgs(Unit sourceUnit, Unit targetUnit, int previousHealth, int newHealth, bool wasKilled)
            {
                this.sourceUnit = sourceUnit;
                this.targetUnit = targetUnit;
                this.previousHealth = previousHealth;
                this.newHealth = newHealth;
                this.wasKilled = wasKilled;
            }
        }
    }
}
