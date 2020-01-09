////////////////////////////////////////////////////////////////////////////////
//
//  PlayerAction (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/28/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using TofuCore;
using UnityEngine;

namespace Tofunaut.GridStrategy.Game
{

    // --------------------------------------------------------------------------------------------
    /// <summary>
    /// A PlayerAction is required to change the state of the game. It contains info about how to
    /// move units, attack units, end a player's turn, etc.
    /// </summary>
    [Serializable]
    public abstract class PlayerAction
    {
        // --------------------------------------------------------------------------------------------
        public enum EType
        {
            Invalid = 0,
            MoveUnit = 1,
            AttackUnit = 2,
            EndTurn = 3,
        }

        public EType type;
        public int playerIndex;

        // --------------------------------------------------------------------------------------------
        protected PlayerAction(EType type, int playerIndex)
        {
            this.type = type;
            this.playerIndex = playerIndex;
        }

        // --------------------------------------------------------------------------------------------
        public virtual bool IsValid(Game game)
        {
            return false;
        }

        public abstract void Execute(Game game, Action OnComplete);
    }

    // --------------------------------------------------------------------------------------------
    /// <summary>
    /// Move the unit occupying one board tile to another.
    /// </summary>
    [Serializable]
    public class MoveAction : PlayerAction
    {
        public int unitId;
        public IntVector2[] path;

        // --------------------------------------------------------------------------------------------
        public MoveAction(int playerIndex, int unitId, IntVector2[] path) : base(EType.MoveUnit, playerIndex)
        {
            this.unitId = unitId;
            this.path = path;
        }

        // --------------------------------------------------------------------------------------------
        public override bool IsValid(Game game)
        {
            Unit toMove = Unit.GetUnit(unitId);

            if (path.Length == 0)
            {
                Debug.LogError("the path is empty");
            }

            if (toMove.HasMoved)
            {
                Debug.LogError($"{toMove.id} has already moved!");
                return false;
            }

            if (!Board.IsPathValid(path))
            {
                Debug.LogError("not a valid path");
                return false;
            }

            if(game.board.CalculatePathCost(path, toMove) > toMove.MoveRange)
            {
                Debug.LogError("path is too expensive");
                return false;
            }

            return true;
        }

        // --------------------------------------------------------------------------------------------
        public override void Execute(Game game, Action OnComplete)
        {
            throw new NotImplementedException();
        }
    }

    // --------------------------------------------------------------------------------------------
    /// <summary>
    /// Have a unit attack another unit.
    /// </summary>
    [Serializable]
    public class AttackAction : PlayerAction
    {
        public int attackerId;
        public int defenderId;

        // --------------------------------------------------------------------------------------------
        public AttackAction(int playerIndex, int attackerId, int defenderId) : base(EType.AttackUnit, playerIndex)
        {
            this.attackerId = attackerId;
            this.defenderId = defenderId;
        }

        // --------------------------------------------------------------------------------------------
        public override void Execute(Game game, Action OnComplete)
        {
            throw new NotImplementedException();
        }
    }

    // --------------------------------------------------------------------------------------------
    /// <summary>
    /// End the player's turn.
    /// </summary>
    [Serializable]
    public class EndTurnAction : PlayerAction
    {
        // --------------------------------------------------------------------------------------------
        public EndTurnAction(int playerIndex) : base(EType.EndTurn, playerIndex) { }

        // --------------------------------------------------------------------------------------------
        public override void Execute(Game game, Action OnComplete)
        {
            game.EndTurn();
            OnComplete?.Invoke();
        }
    }
}
