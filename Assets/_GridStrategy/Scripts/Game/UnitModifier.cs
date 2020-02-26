////////////////////////////////////////////////////////////////////////////////
//
//  Modifier (c) 2020 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 02/23/20
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace Tofunaut.GridStrategy.Game
{
    // --------------------------------------------------------------------------------------------
    public class UnitModifier
    {
        public enum EStackType
        {
            NoStack,
            Add,
            Multiply,
        }

        public event EventHandler<ModifierEventArgs> OnModifierExpired;

        public int TurnsActive { get; private set; }

        public readonly Unit appliedTo;

        protected readonly UnitModifierData _modifierData;

        // --------------------------------------------------------------------------------------------
        public UnitModifier(UnitModifierData modifierData, Game game, Unit appliedTo)
        {
            this.appliedTo = appliedTo;
            appliedTo.Owner.PlayerTurnStarted += OnPlayerTurnStarted;

            _modifierData = modifierData;

            TurnsActive = 0;
        }

        // --------------------------------------------------------------------------------------------
        public void OnPlayerTurnStarted(object sender, Player.PlayerEventArgs e)
        {
            TurnsActive++;

            if (_modifierData.numTurnsActive >= 0 && TurnsActive >= _modifierData.numTurnsActive)
            {
                OnModifierExpired(this, new ModifierEventArgs(this));
            }
        }

        // --------------------------------------------------------------------------------------------
        public class ModifierEventArgs : EventArgs
        {
            public readonly UnitModifier modifier;

            public ModifierEventArgs(UnitModifier modifier)
            {
                this.modifier = modifier;
            }
        }

        // --------------------------------------------------------------------------------------------
        public static UnitModifierTotals CalculateTotals(List<UnitModifier> modifiers)
        {
            // create base modifier totals with default stats.
            UnitModifierTotals toReturn = UnitModifierTotals.Identity;

            return toReturn;
        }
    }

    // --------------------------------------------------------------------------------------------
    public struct UnitModifierTotals
    {
        public float skillDamageAdditive;
        public float skillDamageMultiplier;

        public static UnitModifierTotals Identity => new UnitModifierTotals
        {
            skillDamageAdditive = 0,
            skillDamageMultiplier = 1
        };
    }
}