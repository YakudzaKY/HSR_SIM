using System;
using System.Collections.Generic;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

//Combat was finished
public class FinishCombat : Event
{
    private readonly Dictionary<Unit, List<AppliedBuff>> removedBuffs = new();
    private CombatFight oldCombatFight;
    private Dictionary<Unit, MechDictionary> oldMechDictionary;

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
                ChildEvents.Add(new PartyResourceDrain(ParentStep, this, null)
                {
                    Value = ParentStep.Parent.PartyTeam.GetRes(Resource.ResourceType.SP).ResVal,
                    TargetTeam = ParentStep.Parent.PartyTeam, ResType = Resource.ResourceType.SP
                });
            }


            //save each buff
            foreach (var unit in ParentStep.Parent.PartyTeam.Units)
            {
                if (!TriggersHandled)
                {
                    removedBuffs.Add(unit, unit.AppliedBuffs);
                    var md = new MechDictionary();
                    foreach (var mch in unit.Fighter.Mechanics.Values)
                    {
                        md.AddVal(mch.Key);
                        md.Values[mch.Key] = mch.Value;
                    }

                    oldMechDictionary.Add(unit, md);
                }

                unit.Fighter.Reset();

                foreach (var buff in removedBuffs[unit])
                foreach (var effect in buff.Effects)
                    effect.BeforeRemove(this, buff);

                unit.AppliedBuffs = new List<AppliedBuff>();

                foreach (var buff in removedBuffs[unit])
                foreach (var effect in buff.Effects)
                    effect.OnRemove(this, buff);
            }

            ParentStep.Parent.CurrentFight = null;
        }
        else
        {
            ParentStep.Parent.CurrentFight = oldCombatFight;
            //restore each buff
            foreach (var unitWBuffs in removedBuffs)
            {
                foreach (var buff in unitWBuffs.Value)
                foreach (var effect in buff.Effects)
                    effect.BeforeApply(this, buff);

                unitWBuffs.Key.AppliedBuffs = unitWBuffs.Value;

                foreach (var buff in unitWBuffs.Value)
                foreach (var effect in buff.Effects)
                    effect.OnApply(this, buff);
            }

            foreach (var omd in oldMechDictionary)
            foreach (var mec in omd.Value.Values)
                omd.Key.Fighter.Mechanics.Values[mec.Key] = mec.Value;
        }

        base.ProcEvent(revert);
    }
}