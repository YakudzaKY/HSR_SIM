using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static HSR_SIM_LIB.Step;

namespace HSR_SIM_LIB
{
    /// <summary>
    /// Events. Situation changed when event was proceded
    /// </summary>
    public class Event : CheckEssence
    {
        public delegate double? CalculateValuePrc(Event ent);
        public delegate IEnumerable<Unit> CalculateTargetPrc(Event ent);

        public CalculateValuePrc CalculateValue { get; init; }
        public CalculateTargetPrc CalculateTargets { get; init; }
        private EventType type;
        private Resource.ResourceType resType;
        private Ability abilityValue;
        private double? val;//Theoretical value
        private double? realVal;//Real hit value(cant exceed)

        public bool CanSetToZero { get; init;  } = true;

        public List<Mod> Mods { get; set; } = new List<Mod>();
        public Unit TargetUnit { get; set; }
        public Step.StepTypeEnm OnStepType { get; init; }
        public EventType Type { get => type; set => type = value; }
        public Ability AbilityValue { get => abilityValue; set => abilityValue = value; }
        public double? Val { get => val; set => val = value; }
        public double? RealVal { get => realVal; set => realVal = value; }
        public Resource.ResourceType ResType { get => resType; set => resType = value; }
        public Step ParentStep { get; set; } = null;

        private List<Trigger> triggers = null;

        public List<Trigger> Triggers
        {
            get { return triggers ??= new List<Trigger>(); }
            set => triggers=value;
        } 


        public enum EventType
        {
            CombatStartSkillQueue,
            ResourceDrain,
            PartyResourceDrain,
            EnterCombat,
            StartCombat,
            StartWave,
            Mod,
            ModActionValue,
            ShieldBreak,
            DirectDamage,
            CombatStartSkillDeQueue
        }


        public string GetDescription()
        {
            string res;
            if (Type == EventType.StartCombat)
                res = "Combat was started";
            else if (Type == EventType.CombatStartSkillQueue)
                res = "Queue ability to start skill queue";
            else if (Type == EventType.CombatStartSkillDeQueue)
                res = "Remove ability from start skill queue";
            else if (Type == EventType.PartyResourceDrain)
                res = "Party res drain : " + this.Val + " " + this.ResType.ToString();
            else if (Type == EventType.ResourceDrain)
                res = this.TargetUnit.Name+" res drain : " + this.Val + " " + this.ResType.ToString()+"(by "+this.ParentStep.Actor.Name+")";
            else if (Type == EventType.EnterCombat)
                res = "entering the combat...";
            else if (Type == EventType.Mod)
                res = "Apply modifications";
            else if (Type == EventType.StartWave)
                res = "next wave";
            else if (Type == EventType.DirectDamage)
                res = "Dealing damage";//TODO need expand
            else if (Type == EventType.ShieldBreak)
                res = this.TargetUnit.Name+ " shield broken";
            else
                throw new NotImplementedException();
            return res;
        }
    }
}
