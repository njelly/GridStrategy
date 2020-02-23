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
    public class Modifier
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

        protected readonly ModifierData _modifierData;

        // --------------------------------------------------------------------------------------------
        public Modifier(ModifierData modifierData, Game game, Unit appliedTo)
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
            public readonly Modifier modifier;

            public ModifierEventArgs(Modifier modifier)
            {
                this.modifier = modifier;
            }
        }

        // --------------------------------------------------------------------------------------------
        public static ModifierTotals CalculateTotals(List<Modifier> modifiers)
        {
            // create base modifier totals with default stats.
            ModifierTotals toReturn = ModifierTotals.Identity;

            return toReturn;
        }
    }

    // --------------------------------------------------------------------------------------------
    public struct ModifierTotals
    {
        public float skillDamageAdditive;
        public float skillDamageMultiplier;

        public static ModifierTotals Identity => new ModifierTotals
        {
            skillDamageAdditive = 0,
            skillDamageMultiplier = 1
        };
    }
}