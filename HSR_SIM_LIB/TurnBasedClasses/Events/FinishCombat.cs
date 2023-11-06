using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    //Combat was finished
    internal class FinishCombat : Event
    {
        private Dictionary<Unit, List<Buff>> removedBuffs = new Dictionary<Unit, List<Buff>>();
        private CombatFight oldCombatFight = null;
        private Dictionary<Unit, MechDictionary> oldMechDictionary = null;
        public FinishCombat(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return "finish combat";
        }

        public override void ProcEvent(bool revert)
        {


            if (!revert)
            {

                if (!TriggersHandled)
                {
                    oldCombatFight = ParentStep.Parent.CurrentFight;
                    oldMechDictionary = new Dictionary<Unit, MechDictionary>();
                    //set sp to 0
                    ChildEvents.Add(new PartyResourceDrain(ParentStep, this, null) { Val = ParentStep.Parent.PartyTeam.GetRes(Resource.ResourceType.SP).ResVal, TargetTeam = ParentStep.Parent.PartyTeam, ResType = Resource.ResourceType.SP });

                }



                //save each buff
                foreach (Unit unit in ParentStep.Parent.PartyTeam.Units)
                {


                    if (!TriggersHandled)
                    {
                        removedBuffs.Add(unit, unit.Buffs);
                        MechDictionary md = new MechDictionary();
                        foreach (var mch in ((DefaultFighter)unit.Fighter).Mechanics.Values)
                        {
                            md.AddVal(mch.Key);
                            md.Values[mch.Key] = mch.Value;
                        }
                        oldMechDictionary.Add(unit, md);

                    }
                    unit.Fighter.Reset();

                    foreach (Buff buff in removedBuffs[unit])
                        foreach (Effect effect in buff.Effects)
                        {
                            effect.BeforeRemove(this, buff);
                        }

                    unit.Buffs = new List<Buff>();

                    foreach (Buff buff in removedBuffs[unit])
                        foreach (Effect effect in buff.Effects)
                        {
                            effect.OnRemove(this, buff);
                        }


                }
                ParentStep.Parent.CurrentFight = null;


            }
            else
            {
                ParentStep.Parent.CurrentFight = oldCombatFight;
                //restore each buff
                foreach (var unitWBuffs in removedBuffs)
                {

                    foreach (Buff buff in unitWBuffs.Value)
                        foreach (Effect effect in buff.Effects)
                        {
                            effect.BeforeApply(this, buff);
                        }

                    unitWBuffs.Key.Buffs = unitWBuffs.Value;

                    foreach (Buff buff in unitWBuffs.Value)
                        foreach (Effect effect in buff.Effects)
                        {
                            effect.OnApply(this, buff);
                        }
                }
                foreach (var omd in oldMechDictionary)
                {

                    foreach (var mec in omd.Value.Values)
                    {
                        ((DefaultFighter)omd.Key.Fighter).Mechanics.Values[mec.Key] = mec.Value;
                    }
                }
            }
            base.ProcEvent(revert);
        }
    }
}
