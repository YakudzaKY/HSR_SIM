using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HSR_SIM_LIB.Fighters;
using static HSR_SIM_LIB.TurnBasedClasses.Step;
using static HSR_SIM_LIB.TurnBasedClasses.Event;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Skills;
using static HSR_SIM_LIB.Skills.Effect;
using System.Runtime.Intrinsics.X86;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_LIB.TurnBasedClasses
{
    /// <summary>
    /// Events. Situation changed when event was proceded
    /// </summary>
    public class Event : CloneClass
    {
        public delegate double? CalculateValuePrc(Event ent);
        public delegate IEnumerable<Unit> CalculateTargetPrc();

        public CalculateValuePrc CalculateValue { get; init; }
        public CalculateTargetPrc CalculateTargets { get; init; }
        private EventType type;
        private Resource.ResourceType resType;
        private double? val;//Theoretical value
        private double? realVal;//Real hit value(cant exceed)

        public bool CanSetToZero { get; init; } = true;
        public List<Unit> StartingUnits { get; set; }
        public List<Mod> RemovedMods { get; set; } = new List<Mod>();

        public Mod Modification { get; set; }
        public ICloneable Source { get; }
        public Ability.TargetTypeEnm? TargetType { get; set; }

        public Ability.AbilityCurrentTargetEnm? CurentTargetType { get; set; }
        public Unit SourceUnit { get; set; }
        public Unit TargetUnit { get; set; }
        public StepTypeEnm OnStepType { get; init; }
        public EventType Type { get => type; set => type = value; }
        public Ability AbilityValue { get; set; }
        public double? Val { get => val; set => val = value; }
        public double? RealVal { get => realVal; set => realVal = value; }
        public double? RealBarrierVal { get => realBarrierVal; set => realBarrierVal = value; }
        public bool IsCrit { get; set; }
        public Resource.ResourceType ResType { get => resType; set => resType = value; }
        public Step ParentStep { get; set; } = null;
        public bool TriggersHandled { get; set; } = false;
        private double? realBarrierVal;



        public enum EventType
        {
            CombatStartSkillQueue,// insert technique skill to queue
            ResourceDrain,//drain some resource
            PartyResourceDrain,//drain party resource
            EnterCombat,// command to start battle(when combat technique used)
            StartCombat,//starting the combat
            StartWave,// starting the wave
            Mod,//apply buff debuff dot etc
            ModActionValue,//modify Action Value
            ShieldBreak,//Break the shield
            Defeat,// unit defeated( usefull for geppard etc)
            DirectDamage,// direct damage dealed
            CombatStartSkillDeQueue,// delete skill from queue(when techniqe skill executed in battle)
            DoTDamage,// Damage by DoTs(when turn started)
            FinishCombat,//Combat was finished
            Attack,// Unit made the attack. Good for triggers
            RemoveMod,// dispell buff or dot
            DebuffResisted,//Notify that debuff got resisted
            UnitEnteringBattle,// unit enter on the battlefield
            MechanicValChg//Add value to character mechanic counter
            ,
            ResetAV//reset action value
            ,
            PartyResourceGain,
            ReduceDuration
        }

        public Event(Step parent, ICloneable source, Unit sourceUnit)
        {
            ParentStep = parent;
            Source = source;
            SourceUnit = sourceUnit;
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
                res = "Party res drain : " + Val + " " + ResType.ToString();
            else if (Type == EventType.ResourceDrain)
                res = TargetUnit.Name + " res drain : " + Val + " " + ResType.ToString() + "(by " + SourceUnit.Name + ")";
            else if (Type == EventType.Defeat)
                res = TargetUnit.Name + " get rekt (:";
            else if (Type == EventType.Attack)
                res = AbilityValue.Parent.Name + " doing Attack";
            else if (Type == EventType.EnterCombat)
                res = "entering the combat...";
            else if (Type == EventType.Mod)
                res = $"Apply modifications on {TargetUnit.Name}. Source: {Source?.GetType()?.ToString().Split(".").Last():s}";
            else if (Type == EventType.DebuffResisted)
                res = $"{TargetUnit.Name} debuff resisted: {Modification.Effects.First().EffType}";
            else if (Type == EventType.RemoveMod)
                res = $"Remove modifications on {TargetUnit.Name}. Source: {Source?.GetType()?.ToString().Split(".").Last():s}";
            else if (Type == EventType.StartWave)
                res = "next wave";
            else if (Type == EventType.DirectDamage)
                res = "Dealing damage" + (IsCrit ? " (CRITICAL)" : "") +
                      $" overall={val:f} to_barier={RealBarrierVal:f} to_hp={RealVal:f}";
            else if (Type == EventType.DoTDamage)
                res = $"DoT tick from {SourceUnit.Name}" +
                      $" overall={val:f} to_barier={RealBarrierVal:f} to_hp={RealVal:f}";
            else if (Type == EventType.ShieldBreak)
                res = TargetUnit.Name + " shield broken " +
                      $" overall={val:f} to_hp={RealVal:f}";
            else if (Type == EventType.ModActionValue)
                res = $"Reduce {TargetUnit?.Name:s} action value on {Val:f}";
            else if (Type == EventType.UnitEnteringBattle)
                res = $"{TargetUnit?.Name:s} joined the battle";
            else if (Type == EventType.MechanicValChg)
                res = $"{TargetUnit?.Name:s} mechanic counter change on  {Val:f}";
            else if (Type == EventType.ResetAV)
                res = $"{TargetUnit?.Name:s} reset action value";
            else if (Type == EventType.PartyResourceGain)
                res = $"Party res gain :  {Val:f} {ResType} by {TargetUnit.Name}";
            else if (Type == EventType.ReduceDuration)
                res = null;
            else
                throw new NotImplementedException();
            return res;
        }

        /// <summary>
        /// Proc one event
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="revert"></param>
        public void ProcEvent(bool revert)
        {
            ParentStep.ProceedEvents.Add(this);
            //calc value first
            if (CalculateValue != null && Val == null)
                Val = CalculateValue(this);
            if (Type == EventType.PartyResourceDrain)//SP or technical points
            {
                SourceUnit.ParentTeam.GetRes(ResType).ResVal -= (double)(revert ? -Val : Val);
            }
            else if (Type == EventType.PartyResourceGain)//SP or technical points
            {
                double CurrentResVal = TargetUnit.ParentTeam.GetRes(ResType).ResVal;
                if (ResType == Resource.ResourceType.SP)
                {
                    if (CurrentResVal + Val > Constant.MaxSp)
                        Val = Constant.MaxSp - CurrentResVal;
                }
                if (ResType == Resource.ResourceType.TP)
                {
                    if (Val + CurrentResVal > Constant.MaxTp)
                        Val = Constant.MaxTp - CurrentResVal;
                }
                TargetUnit.ParentTeam.GetRes(ResType).ResVal += (double)(revert ? -Val : Val);
            }
            else if (Type == EventType.CombatStartSkillQueue)//party buffs or opening
                if (!revert)
                    ParentStep.Parent.BeforeStartQueue.Add(AbilityValue);
                else
                    ParentStep.Parent.BeforeStartQueue.Remove(AbilityValue);
            else if (Type == EventType.CombatStartSkillDeQueue)//DEQUEUE party buffs or opening
                if (!revert)
                    ParentStep.Parent.BeforeStartQueue.Remove(AbilityValue);
                else
                    ParentStep.Parent.BeforeStartQueue.Add(AbilityValue);
            else if (Type == EventType.EnterCombat)//entering combat
                ParentStep.Parent.DoEnterCombat = !revert;
            else if (Type == EventType.FinishCombat) //Finish combat
            {

                //todo: reset all passives and counters for characters
                //also reset for gear and light cones
                //
                throw new NotImplementedException();
            }
            else if (Type == EventType.StartCombat)//Loading combat
            {
                if (!revert)
                {
                    ParentStep.Parent.DoEnterCombat = false;
                    ParentStep.Parent.CurrentFight = new CombatFight(ParentStep.Parent.CurrentScenario.Fights[ParentStep.Parent.CurrentFightStep], ParentStep.Parent);
                    ParentStep.Parent.CurrentFightStep += 1;
                }
                else
                {
                    ParentStep.Parent.DoEnterCombat = true;
                    ParentStep.Parent.CurrentFight = null;
                    ParentStep.Parent.CurrentFightStep -= 1;
                }

            }
            else if (Type == EventType.ModActionValue) //SetAV
            {
                //if no target - reduce all units
                if (TargetUnit != null)
                {
                    TargetUnit.Stats.PerformedActionValue += (double)(revert ? -Val : Val);
                }
                else
                {

                    foreach (Unit unit in ParentStep.Parent.CurrentFight.AllAliveUnits)
                    {
                        unit.Stats.PerformedActionValue += (double)(revert ? -Val : Val);
                    }
                }
            }
            else if (Type == EventType.MechanicValChg) //SetAV
            {

                ((DefaultFighter)TargetUnit.Fighter).Mechanics.Values[AbilityValue] += (double)(revert ? -Val : Val);
            }
            else if (Type == EventType.ResetAV) //Loading wave
            {
                TargetUnit.Stats.ResetAV();
            }
            else if (Type == EventType.StartWave)//Loading wave
            {
                if (!revert)
                {
                    ParentStep.Parent.CurrentFight.CurrentWaveCnt += 1;
                    ParentStep.Parent.CurrentFight.CurrentWave = ParentStep.Parent.CurrentFight.ReferenceFight.Waves[ParentStep.Parent.CurrentFight.CurrentWaveCnt - 1];
                    StartingUnits ??= SimCls.GetCombatUnits(ParentStep.Parent.CurrentFight.CurrentWave.Units);
                    ParentStep.Parent.HostileTeam.BindUnits(StartingUnits);
                    //set start action value
                    foreach (Unit unit in ParentStep.Parent.AllUnits)
                    {
                        unit.Stats.ResetAV();
                    }

                }
                else
                {
                    ParentStep.Parent.HostileTeam.UnBindUnits();
                    ParentStep.Parent.CurrentFight.CurrentWaveCnt -= 1;
                    if (ParentStep.Parent.CurrentFight.CurrentWaveCnt > 0)
                        ParentStep.Parent.CurrentFight.CurrentWave = ParentStep.Parent.CurrentFight.ReferenceFight.Waves[ParentStep.Parent.CurrentFight.CurrentWaveCnt - 1];
                    else
                    {
                        ParentStep.Parent.CurrentFight.CurrentWave = null;
                    }

                }

            }
            else if (Type == EventType.Mod) //Apply mod
            {
                if (TargetUnit.IsAlive)
                {
                    //calc value first
                    foreach (Effect modEffect in Modification.Effects)
                    {
                        if (modEffect.CalculateValue != null && modEffect.Value == null)
                            modEffect.Value = modEffect.CalculateValue(this);
                    }


                    if (!revert)
                        TargetUnit.ApplyMod(Modification);
                    else
                        TargetUnit.RemoveMod(Modification);
                }

            }
            else if (Type == EventType.ReduceDuration) //reduce MOD turns durationleft
            {

                Modification.DurationLeft -= !revert ? 1 : -1;
                if (!TriggersHandled && Modification.DurationLeft <= 0)
                {
                    DispelMod(Modification,true);

                }


            }
            else if (Type == EventType.RemoveMod) //remove mod
            {

                if (!revert)
                    TargetUnit.RemoveMod(Modification);
                else
                    TargetUnit.ApplyMod(Modification);

            }
            else if (Type == EventType.ResourceDrain) //Resource drain
            {
                if (RealVal == null)
                {
                    RealVal = Math.Min((double)TargetUnit.GetRes(ResType).ResVal, (double)Val);
                    if (!CanSetToZero && RealVal < Val)
                    {
                        RealVal -= 1;
                    }
                }
                SetResByEvent(ResType, (double)-(revert ? -RealVal : RealVal));
            }
            else if (Type == EventType.Defeat) //got defeated
            {
                TargetUnit.IsAlive = revert;

                if (!revert)
                    foreach (Mod mod in RemovedMods)
                    {
                        TargetUnit.RemoveMod(mod);
                    }
                else
                    foreach (Mod mod in RemovedMods)
                    {
                        TargetUnit.ApplyMod(mod);
                    }


            }
            else if (Type == EventType.DebuffResisted) //Debuf resisted :(
            {
                //Event handlers handle this event
            }
            else if (Type == EventType.Attack) //Just doing attack
            {
                //Event handlers handle this event
            }
            else if (Type == EventType.UnitEnteringBattle) //unit entering the battle
            {
                //Event handlers handle this event
            }
            else if (Type is EventType.DirectDamage or EventType.ShieldBreak or EventType.DoTDamage)
            {
                if (RealVal == null)
                {
                    //find the all shields and max value
                    double maxShieldval=0;
                    double srchVal;
                    foreach (Mod mod in TargetUnit.Mods.Where(x=>x.Effects.Any(y=>y.EffType==EffectType.Shield)))
                    {
                        foreach (Effect eff in mod.Effects.Where(x=>x.EffType==EffectType.Shield))
                        {
                            srchVal = eff.Value ?? 0;
                            if (srchVal > maxShieldval)
                            {
                                maxShieldval = srchVal; 
                            }
                        }
                       
                    }
                    //cant hit more than val
                    RealBarrierVal = Math.Min((double)maxShieldval,
                        (double)Val);
                    //get current hp
                    var resVal = TargetUnit.GetRes(Resource.ResourceType.HP).ResVal;
                    //same shit here
                    RealVal = Math.Min((double)resVal,
                        (double)Val - (double)RealBarrierVal);
                }

                //reduce all shields
                foreach (Mod mod in TargetUnit.Mods.Where(x => x.Effects.Any(y => y.EffType == EffectType.Shield)))
                {
                    foreach (Effect eff in mod.Effects.Where(x=>x.EffType==EffectType.Shield))
                    {
                        eff.Value-= (revert ? -RealBarrierVal : RealBarrierVal);
                    }
                }

                //need create event once
                if (!TriggersHandled)
                {
                    List<Mod> toDispell = new List<Mod>();
                    toDispell.AddRange(TargetUnit.Mods.Where(x =>
                        x.Effects.Any(y => y.EffType == EffectType.Shield && y.Value <= 0)));
                    //dispell zero shields
                    foreach (Mod mod in toDispell)
                    {
                        DispelMod(mod,true);
                        
                    }
                }

                SetResByEvent(Resource.ResourceType.HP, (double)-(revert ? -RealVal : RealVal));
            }
            else
                throw new NotImplementedException();

            //call handlers
            if (!TriggersHandled)
            {
                TriggersHandled = true;
                ParentStep.Parent.EventHandlerProc?.Invoke(this);

            }


        }

        /// <summary>
        /// Remove modifictaion
        /// </summary>
        /// <param name="mod">Modification</param>
        /// <param name="naturalFinish">If true - MOD exceed by duratiuon. If false - dispeleed by ability</param>
        /// <exception cref="NotImplementedException"></exception>
        private void DispelMod(Mod mod,bool naturalFinish )
        {
            if (naturalFinish)
            {
                mod.ProceedExpire(this);
            }
            Event dispell = new Event(this.ParentStep, this.AbilityValue,SourceUnit)
                { type = EventType.RemoveMod, AbilityValue = this.AbilityValue, Modification = mod, TargetUnit = TargetUnit };
            dispell.ProcEvent(false);
            ParentStep.Events.Add(dispell);
            
        }


        /// <summary>
        /// attempt to apply debuff
        /// </summary>
        /// <param name="modType">What we modificate</param>
        /// <param name="effects">effect list </param>
        /// <param name="baseDuration">Duration of the mod</param>
        /// <param name="baseChance">base chance of debuff</param>
        /// <param name="maxStack">max stacks</param>
        /// <param name="uniqueStr">Unique buff per battle</param>
        /// <param name="uniqueUnit">Unique buff per unit</param>
        private void TryDebuff(Mod.ModType modType, List<Effect> effects, int baseDuration, double baseChance, int maxStack = 1, string uniqueStr = "", Unit uniqueUnit = null)
        {
            //add Dots and debuffs
            Event dotEvent = new(ParentStep, this.Source,SourceUnit)
            {
                Type = EventType.Mod,
                AbilityValue = AbilityValue,
                TargetUnit = TargetUnit,
                BaseChance = baseChance,
                Modification = new Mod(SourceUnit) { DoNotClone= true ,Type = modType, BaseDuration = baseDuration, Effects = effects, MaxStack = maxStack, UniqueStr = uniqueStr, UniqueUnit = uniqueUnit }
            };


            if (FighterUtils.CalculateDebuffResisted(dotEvent))
            {
                ParentStep.Events.Add(dotEvent);
                //subscription to events(need calc stacks at attacks)
                if (dotEvent.Modification.Effects.Any(x=>x.EffType==EffectType.Entanglement))
                    dotEvent.Modification.EventHandlerProc += dotEvent.Modification.EntanglementEventHandler;
                dotEvent.ProcEvent(false);
            }
            else
            {
                //debuff apply failed
                Event failEvent = new(ParentStep, this.Source, SourceUnit)
                {
                    Type = EventType.DebuffResisted,
                    AbilityValue = AbilityValue,
                    TargetUnit = TargetUnit,
                    Modification = dotEvent.Modification

                };
                ParentStep.Events.Add(failEvent);
                failEvent.ProcEvent(false);
            }
        }
        //Base Chance to apply debuff 
        public double BaseChance { get; set; }

        /// <summary>
        /// Set resource and do some new events
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="chgVal"></param>
        private void SetResByEvent(Resource.ResourceType resourceType, double chgVal)
        {
            Resource res = TargetUnit.GetRes(resourceType);
            if (chgVal != 0)
            {
                double newVal = res.ResVal + chgVal;
                ///BEFORE SET!
                if (!TriggersHandled)
                {
                    if (newVal == 0)
                    {
                        if (res.ResType == Resource.ResourceType.Toughness)
                        {


                            Event shieldBrkEvent = new(ParentStep, this.Source, SourceUnit)
                            {
                                Type = EventType.ShieldBreak,
                                AbilityValue = AbilityValue,
                                TargetUnit = TargetUnit

                            };
                            shieldBrkEvent.Val = FighterUtils.CalculateShieldBrokeDmg(shieldBrkEvent);
                            ParentStep.Events.Add(shieldBrkEvent);
                            shieldBrkEvent.ProcEvent(false);

                            Event delayAV = new (ParentStep, this.Source, SourceUnit) { Type = EventType.ModActionValue, TargetUnit = TargetUnit, Val = -TargetUnit.Stats.BaseActionValue * 0.25 };//default delay
                            ParentStep.Events.Add(delayAV);
                            delayAV.ProcEvent(false);
                            //TODO https://honkai-star-rail.fandom.com/wiki/Toughness need implement additional effects
                            switch (this.AbilityValue.Element?? SourceUnit.Fighter.Element)
                            {
                                case Unit.ElementEnm.Physical:
                                    TryDebuff(Mod.ModType.Dot, new List<Effect>() { new Effect() { EffType = EffectType.Bleed, CalculateValue = FighterUtils.CalculateShieldBrokeDmg } }, 2, 1.5);
                                    break;
                                case Unit.ElementEnm.Fire:
                                    TryDebuff(Mod.ModType.Dot, new List<Effect>() { new Effect() { EffType = EffectType.Burn, CalculateValue = FighterUtils.CalculateShieldBrokeDmg } }, 2, 1.5);
                                    break;
                                case Unit.ElementEnm.Ice:
                                    TryDebuff(Mod.ModType.Debuff, new List<Effect>() { new Effect() { EffType = EffectType.Freeze, CalculateValue = FighterUtils.CalculateShieldBrokeDmg } }, 1, 1.5);
                                    break;
                                case Unit.ElementEnm.Lightning:
                                    TryDebuff(Mod.ModType.Dot, new List<Effect>() { new Effect() { EffType = EffectType.Shock, CalculateValue = FighterUtils.CalculateShieldBrokeDmg } }, 2, 1.5);
                                    break;
                                case Unit.ElementEnm.Wind:
                                    TryDebuff(Mod.ModType.Dot, new List<Effect>() { new Effect() { EffType = EffectType.WindShear, CalculateValue = FighterUtils.CalculateShieldBrokeDmg } }, 2, 1.5, 5, uniqueUnit: SourceUnit);
                                    break;
                                case Unit.ElementEnm.Quantum:
                                    TryDebuff(Mod.ModType.Debuff, new List<Effect>() {
                                        new Effect(){EffType=EffectType.Entanglement,CalculateValue = FighterUtils.CalculateShieldBrokeDmg}
                                        ,new Effect(){EffType=EffectType.Delay,Value = 0.20*(1+SourceUnit.Stats.BreakDmg) ,StackAffectValue = false}
                                    }, 1, 1.5, 5);
                                    break;
                                case Unit.ElementEnm.Imaginary:
                                    TryDebuff(Mod.ModType.Debuff, new List<Effect>() {
                                        new Effect(){EffType=EffectType.Imprisonment,CalculateValue = FighterUtils.CalculateShieldBrokeDmg}
                                        ,new Effect(){EffType=EffectType.Delay,Value = 0.30*(1+SourceUnit.Stats.BreakDmg)}
                                        ,new Effect(){EffType=EffectType.ReduceSpdPrc,Value = 0.1}
                                    }, 1, 1.5, 1);
                                    break;
                                default:
                                    throw new NotImplementedException();

                            }

                        }


                    }


                }

                res.ResVal += chgVal;
                /// AFTER VALUE SET!
                //set to zero
                if (!TriggersHandled)
                {
                    if (newVal == 0)
                    {
                        if (res.ResType == Resource.ResourceType.HP)
                        {

                            Event defeatEvent = new(ParentStep, this.Source, SourceUnit)
                            {
                                Type = EventType.Defeat,
                                AbilityValue = AbilityValue,
                                TargetUnit = TargetUnit

                            };
                            //remove all buffs and debuffs
                            defeatEvent.RemovedMods.AddRange(TargetUnit.Mods);
                            ParentStep.Events.Add(defeatEvent);
                            defeatEvent.ProcEvent(false);

                        }
                    }
                }



            }


        }
    }

}
