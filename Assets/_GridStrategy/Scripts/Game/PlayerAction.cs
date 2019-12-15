////////////////////////////////////////////////////////////////////////////////
//
//  PlayerAction (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/28/2019
//
////////////////////////////////////////////////////////////////////////////////

using System;
using TofuCore;

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
        public IntVector2 toCoord;

        // --------------------------------------------------------------------------------------------
        public MoveAction(int playerIndex, int unitId, IntVector2 toCoord) : base(EType.MoveUnit, playerIndex)
        {
            this.unitId = unitId;
            this.toCoord = toCoord;
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
