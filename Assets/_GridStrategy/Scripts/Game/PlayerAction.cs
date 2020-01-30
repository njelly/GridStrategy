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
            UseSkill = 2,
            EndTurn = 3,
            PlayCard = 4,
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

            foreach(IntVector2 pathPoint in path)
            {
                Debug.Log(pathPoint.ToString());
            }

            Debug.Log(game.board.CalculatePathCost(path, toMove));

            if(game.board.CalculatePathCost(path, toMove) > toMove.MoveRange)
            {
                Debug.LogError("path is too expensive");
                return false;
            }

            return true;
        }

        // --------------------------------------------------------------------------------------------
        public override void Execute(Game game, Action onComplete)
        {
            Unit toMove = Unit.GetUnit(unitId);
            toMove.Move(path, true, onComplete);
        }
    }

    // --------------------------------------------------------------------------------------------
    /// <summary>
    /// Have a unit attack another unit.
    /// </summary>
    [Serializable]
    public class UseSkillAction : PlayerAction
    {
        public int unitId;
        public int facingDir;
        public IntVector2 targetCoord;

        // --------------------------------------------------------------------------------------------
        public UseSkillAction(int playerIndex, int unitId, Unit.EFacing facingDir, IntVector2 targetCoord) : base(EType.UseSkill, playerIndex)
        {
            this.unitId = unitId;
            this.facingDir = (int)facingDir;
            this.targetCoord = targetCoord;
        }

        // --------------------------------------------------------------------------------------------
        public override bool IsValid(Game game)
        {
            Unit unit = Unit.GetUnit(unitId);
            if(unit.HasUsedSkill)
            {
                return false;
            }

            return true;
        }

        // --------------------------------------------------------------------------------------------
        public override void Execute(Game game, Action OnComplete)
        {
            Unit unit = Unit.GetUnit(unitId);
            unit.UseSkill((Unit.EFacing)facingDir, targetCoord, OnComplete);
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
        public override bool IsValid(Game game)
        {
            return game.CurrentPlayer.playerIndex == playerIndex;
        }

        // --------------------------------------------------------------------------------------------
        public override void Execute(Game game, Action OnComplete)
        {
            game.EndTurn();
            OnComplete?.Invoke();
        }
    }

    // --------------------------------------------------------------------------------------------
    /// <summary>
    /// End the player's turn.
    /// </summary>
    public class PlayCardAction : PlayerAction
    {
        public int cardId;
        public IntVector2 coord;

        public PlayCardAction(int playerIndex, int cardId, IntVector2 coord) : base(EType.PlayCard, playerIndex)
        {
            this.cardId = cardId;
            this.coord = coord;
        }

        // --------------------------------------------------------------------------------------------
        public override bool IsValid(Game game)
        {
            Card card = Card.GetCard(cardId);
            BoardTile boardTile = game.board.GetTile(coord);
            if(boardTile == null)
            {
                return false;
            }

            if(!card.CanPlayOnTile(boardTile))
            {
                return false;
            }

            if(card.energyRequired > card.Owner.Energy)
            {
                return false;
            }

            return true;
        }

        // --------------------------------------------------------------------------------------------
        public override void Execute(Game game, Action OnComplete)
        {
            Card card = Card.GetCard(cardId);
            BoardTile boardTile = game.board.GetTile(coord);

            card.Owner.PlayCard(card, boardTile);
        }
    }
}
