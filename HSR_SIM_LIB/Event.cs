﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{
    /// <summary>
    /// Events. Situation changed when event was proceded
    /// </summary>
    public class Event:CheckEssence
    {
        private EventType type;
        private Resource.ResourceType resType;
        private Ability abilityValue;
        public List<Mod> Mods { get; init; } = new List<Mod>();
        public Step.StepTypeEnm OnStepType { get; init; }
        public EventType Type { get => type; set => type = value; }
        public Ability AbilityValue { get => abilityValue; set => abilityValue = value; }
        public int ResourceValue { get => resourceValue; set => resourceValue = value; }
        public Resource.ResourceType ResType { get => resType; set => resType = value; }

        private int resourceValue;

        public enum EventType
        {
            CombatStartSkillQueue,
            CombatStartSkillDeQueue,
            ResourceDrain,
            PartyResourceDrain,
            EnterCombat,
            StartCombat,
            StartWave,
            Mod,//modificer buff etc
            ModActionValue//reduce action value
        }


    }
}
