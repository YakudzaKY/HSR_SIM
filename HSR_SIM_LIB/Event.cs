﻿using System;
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
        private EventType type;
        private Resource.ResourceType resType;
        private Ability abilityValue;
        public bool NeedCalc { get; init; } = false;

        public List<Mod> Mods { get; init; } = new List<Mod>();
        public Unit TargetUnit { get; set; }
        public Ability.TargetTypeEnm TargetType { get; init; }
        public Step.StepTypeEnm OnStepType { get; init; }
        public EventType Type { get => type; set => type = value; }
        public Ability AbilityValue { get => abilityValue; set => abilityValue = value; }
        public int Val { get => val; set => val = value; }
        public string StrValue { get => strValue; set => strValue = value; }
        public Resource.ResourceType ResType { get => resType; set => resType = value; }

        private int val;
        private string strValue;


        /// <summary>
        /// Calculating the Event values
        /// </summary>
        /// <param name="parentStep"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Calc(Step parentStep)
        {
            if (NeedCalc)
            {

                if (StrValue != null)
                {
                    string[] words = StrValue.Split('.');
                    Unit who = null;
                    int? what = null;
                    int? prcMulti = null;
                    //Go calc
                    foreach (var word in words)
                    {
                        //WHO
                        if (who == null)
                        {
                            if (word == Ability.TargetTypeEnm.Self.ToString())
                            {
                                who = parentStep.Actor;
                                continue;
                            }
                            else if (word == Ability.TargetTypeEnm.Target.ToString())
                            {
                                who = this.TargetUnit;
                                continue;
                            }
                        }

                        //WHAT
                        else if (who != null && what == null)
                        {
                            UnitStats unitstats = (UnitStats)who.GetType()
                                .GetProperty("Stats")
                                .GetValue(who, null);
                            //try stats
                            what = unitstats.GetType().GetProperty(word)?.GetValue(unitstats, null) as int?;
                            //try resources
                            if (what == null)
                            {
                                what = who.GetRes(
                                    (Resource.ResourceType)Enum.Parse(typeof(Resource.ResourceType), word)).ResVal;
                            }
                        }
                        else
                        {
                            if (word.EndsWith('%'))
                            {
                                prcMulti = int.Parse(word[..^1]);
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                        }
                    }

                    if (who != null & what != null)
                    {
                        Val = (int) Math.Round ((double)what * ((double)prcMulti /100));
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                }
                else
                {
                    throw new NotImplementedException();
                }

                //Val = (int)Math.Round((double)(CurHp) * (double)int.Parse(StrCost.Substring(0, StrCost.Length - 1)) / 100);
                //Val = Math.Min(Val, CurHp - 1);


            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public enum EventType
        {
            CombatStartSkillQueue,
            CombatStartSkillDeQueue,
            ResourceDrain,
            PartyResourceDrain,
            EnterCombat,
            StartCombat,
            StartWave,
            Mod,
            ModActionValue,
            ToughnessDamage,
            DirectDamage
        }


        public string GetDescription()
        {
            string res;
            if (Type == EventType.StartCombat)
                res = "Combat was started";
            else if (Type == EventType.CombatStartSkillQueue)
                res = "Queue ability to start skill queue";
            else if (Type == EventType.CombatStartSkillDeQueue)
                res = "Remove ability to start skill queue";
            else if (Type == EventType.PartyResourceDrain)
                res = "Party res drain : " + this.Val + " " + this.ResType.ToString();
            else if (Type == EventType.ResourceDrain)
                res = "Unit res drain : " + this.Val + " " + this.ResType.ToString();
            else if (Type == EventType.EnterCombat)
                res = "entering the combat...";
            else if (Type == EventType.Mod)
                res = "Apply modifications";
            else if (Type == EventType.StartWave)
                res = "next wave";
            else
                throw new NotImplementedException();
            return res;
        }
    }
}
