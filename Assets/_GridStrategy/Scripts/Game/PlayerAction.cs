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

            if(path.Length == 0)
            {
                Debug.LogError("the path is empty");
            }

            if(toMove.HasMoved)
            {
                Debug.LogError($"{toMove.id} has already moved!");
                return false;
            }

            if(!toMove.BoardTile.IsAdjacentTo(game.board[path[0].x, path[0].y]))
            {
                Debug.LogError($"{toMove.id} must be on a board tile adjacent to the first tile in the path");
                return false;
            }

            int cost = 0;
            for (int i = 0; i < path.Length; i++)
            {
                if (i >= path.Length - 1)
                {
                    break;
                }

                if ((path[i] - path[i + 1]).ManhattanDistance != 1)
                {
                    Debug.LogError("the path is not continuous!");
                    return false;
                }

                cost += game.board[path[i].x, path[i].y].GetMoveCostForUnit(toMove);
            }

            if(cost > toMove.MoveRange)
            {
                Debug.Log($"{cost} is greater than {toMove.MoveRange}");
                return false;
            }

            return true;
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
    }
}
