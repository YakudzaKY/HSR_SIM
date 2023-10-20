using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HSR_SIM_LIB.Fighters;
using static HSR_SIM_LIB.TurnBasedClasses.Step;
using static HSR_SIM_LIB.TurnBasedClasses.Event;
using static HSR_SIM_LIB.UnitStuff.Resource;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Skills;

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
        private ResourceType resType;
        private double? val;//Theoretical value
        private double? realVal;//Real hit value(cant exceed)

        public bool CanSetToZero { get; init; } = true;
        public List<Unit> StartingUnits { get; set; }
        public List<Mod> Mods { get; set; } = new List<Mod>();
        public ICloneable Source { get; }
        public Unit TargetUnit { get; set; }
        public StepTypeEnm OnStepType { get; init; }
        public EventType Type { get => type; set => type = value; }
        public Ability AbilityValue { get; set; }
        public double? Val { get => val; set => val = value; }
        public double? RealVal { get => realVal; set => realVal = value; }
        public double? RealBarrierVal { get => realBarrierVal; set => realBarrierVal = value; }
        public bool IsCrit { get; set; }
        public ResourceType ResType { get => resType; set => resType = value; }
        public Step ParentStep { get; set; } = null;
        public bool TriggersHandled { get; set; } = false;
        private double? realBarrierVal;


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
            Defeat,
            DirectDamage,
            CombatStartSkillDeQueue,
            DoTDamage,
            DoTPlace,
            FinishCombat,
            Attack,
            RemoveMod
        }

        public Event(Step parent, ICloneable source)
        {
            ParentStep = parent;
            Source = source;
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
                res = TargetUnit.Name + " res drain : " + Val + " " + ResType.ToString() + "(by " + ParentStep.Actor.Name + ")";
            else if (Type == EventType.Defeat)
                res = TargetUnit.Name + " get rekt (:";
            else if (Type == EventType.Attack)
                res = AbilityValue.Parent.Name + " doing Attack";
            else if (Type == EventType.EnterCombat)
                res = "entering the combat...";
            else if (Type == EventType.Mod)
                res = $"Apply modifications on {TargetUnit.Name}. Source: {Source?.GetType()?.ToString().Split(".").Last():s}" ;
            else if (Type == EventType.RemoveMod)
                res = $"Remove modifications on {TargetUnit.Name}. Source: {Source?.GetType()?.ToString().Split(".").Last():s}";
            else if (Type == EventType.StartWave)
                res = "next wave";
            else if (Type == EventType.DirectDamage)
                res = "Dealing damage" + (IsCrit ? " (CRITICAL)" : "") +
                      $" overall={val:f} to_barier={RealBarrierVal:f} to_hp={RealVal:f}";
            else if (Type == EventType.ShieldBreak)
                res = TargetUnit.Name + " shield broken " +
                      $" overall={val:f} to_hp={RealVal:f}";
            else if (Type == EventType.ModActionValue)
                res = $"Reduce {TargetUnit?.Name:s} action value on {(int)Val:d}";
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
                ParentStep.Actor.ParentTeam.GetRes(ResType).ResVal -= (double)(revert ? -Val : Val);
            }
            else if (Type == EventType.CombatStartSkillQueue)//party buffs or opening
                if (!revert)
                    ParentStep.Parent.BeforeStartQueue.Add(ParentStep.ActorAbility);
                else
                    ParentStep.Parent.BeforeStartQueue.Remove(ParentStep.ActorAbility);
            else if (Type == EventType.CombatStartSkillDeQueue)//DEQUEUE party buffs or opening
                if (!revert)
                    ParentStep.Parent.BeforeStartQueue.Remove(ParentStep.ActorAbility);
                else
                    ParentStep.Parent.BeforeStartQueue.Add(ParentStep.ActorAbility);
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
                    TargetUnit.Stats.ActionValue -= (double)(revert ? -Val : Val);
                }
                else
                {

                    foreach (Unit unit in ParentStep.Parent.CurrentFight.AllAliveUnits)
                    {
                        unit.Stats.ActionValue -= (double)(revert ? -Val : Val);
                    }
                }
            }
            else if (Type == EventType.StartWave)//Loading wave
            {
                if (!revert)
                {
                    //TODO reset start wave passives
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
                foreach (var mod in Mods.Where(mod => TargetUnit.IsAlive))
                {
                    //calc value first
                    if (mod.CalculateValue != null && mod.Value == null)
                        mod.Value = mod.CalculateValue(this);

                    if (!revert)
                        TargetUnit.ApplyMod(mod);
                    else
                        TargetUnit.RemoveMod(mod);
                }
            }
            else if (Type == EventType.RemoveMod) //remove mod
            {

                foreach (var mod in Mods)
                {
                    if (!revert)
                        TargetUnit.RemoveMod(mod);
                    else
                        TargetUnit.ApplyMod(mod);


                }

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
                foreach (Mod mod in Mods)
                    if (!revert)
                        TargetUnit.RemoveMod(mod);
                    else
                        TargetUnit.ApplyMod(mod);


            }
            else if (Type == EventType.Attack) //Just doing attack
            {
                //Event handlers handle this event
            }
            else if (Type == EventType.DirectDamage || Type == EventType.ShieldBreak) //Direct damage
            {
                if (RealVal == null)
                {
                    RealBarrierVal = Math.Min((double)TargetUnit.GetRes(ResourceType.Barrier).ResVal,
                        (double)Val);

                    var resVal = TargetUnit.GetRes(ResourceType.HP).ResVal;
                    if (resVal != null)
                        RealVal = Math.Min((double)resVal,
                            (double)Val - (double)RealBarrierVal);
                }

                SetResByEvent(ResourceType.Barrier, (double)-(revert ? -RealBarrierVal : RealBarrierVal));
                SetResByEvent(ResourceType.HP, (double)-(revert ? -RealVal : RealVal));
            }
            else
                throw new NotImplementedException();

            //call handlers
            if (!TriggersHandled)
            {
                TriggersHandled = true;
                foreach (Unit unit in ParentStep.Parent.PartyTeam.Units)
                    unit.Fighter.EventHandlerProc.Invoke(this);
                if (ParentStep.Parent?.HostileTeam?.Units != null)
                    foreach (Unit unit in ParentStep.Parent.HostileTeam.Units)
                        unit.Fighter.EventHandlerProc.Invoke(this);
            }


        }

        /// <summary>
        /// Set resource and do some new events
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="chgVal"></param>
        private void SetResByEvent(ResourceType resourceType, double chgVal)
        {
            Resource res = TargetUnit.GetRes(resourceType);
            if (chgVal != 0)
            {
                res.ResVal += chgVal;
                //set to zero
                if (!TriggersHandled)
                {
                    if (res.ResVal == 0)
                    {
                        if (res.ResType == ResourceType.Toughness)
                        {


                            Event shieldBrkEvent = new(ParentStep, this.Source)
                            {
                                Type = EventType.ShieldBreak,
                                AbilityValue = AbilityValue,
                                TargetUnit = TargetUnit

                            };
                            shieldBrkEvent.Val = FighterUtils.CalculateShieldBrokeDmg(shieldBrkEvent);
                            ParentStep.Events.Add(shieldBrkEvent);
                            shieldBrkEvent.ProcEvent(false);
                            //add Dots and debuffs
                            Event dotEvent = new(ParentStep, this.Source)
                            {
                                Type = EventType.Mod,
                                AbilityValue = AbilityValue,
                                TargetUnit = TargetUnit

                            };
                            switch (ParentStep.ActorAbility.Element??ParentStep.Actor.Fighter.Element)
                            {
                                case Unit.ElementEnm.Physical:
                                    dotEvent.Mods.Add(new Mod(null) {Type=Mod.ModType.Dot ,CalculateValue = FighterUtils.CalculateShieldBrokeDmg} );
                                    break;
                                case Unit.ElementEnm.Fire:
                                   
                                    break;
                                case Unit.ElementEnm.Ice:
                                    
                                    break;
                                case Unit.ElementEnm.Lightning:
                                  
                                    break;
                                case Unit.ElementEnm.Wind:
                                 
                                    break;
                                case Unit.ElementEnm.Quantum:
                                   
                                    break;
                                case Unit.ElementEnm.Imaginary:
                                   
                                    break;
                                default:
                                    throw new NotImplementedException();
                                    break;
                            }
                            ParentStep.Events.Add(dotEvent);
                            dotEvent.ProcEvent(false);
                        }
                        else if (res.ResType == ResourceType.HP)
                        {

                            Event defeatEvent = new(ParentStep, this.Source)
                            {
                                Type = EventType.Defeat,
                                AbilityValue = AbilityValue,
                                TargetUnit = TargetUnit

                            };
                            //remove all buffs and debuffs
                            defeatEvent.Mods.AddRange(TargetUnit.Mods);
                            ParentStep.Events.Add(defeatEvent);
                            defeatEvent.ProcEvent(false);

                        }

                    }


                }
            }


        }
    }

}
