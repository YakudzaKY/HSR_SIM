using System;
using System.Linq;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

//starting the combat
internal class StartCombat(Step parent, ICloneable source, Unit sourceUnit) : Event(parent, source, sourceUnit)
{
    public override string GetDescription()
    {
        return "Combat was started";
    }

    public override void ProcEvent(bool revert)
    {
        //Loading combat

        if (!revert)
        {
            ParentStep.Parent.DoEnterCombat = false;
            ParentStep.Parent.CurrentFight =
                new CombatFight(ParentStep.Parent.CurrentScenario.Fights[ParentStep.Parent.CurrentFightStep],
                    ParentStep.Parent);
            ParentStep.Parent.CurrentFightStep += 1;
            if (!TriggersHandled)
                ChildEvents.Add(new PartyResourceGain(ParentStep, this, null)
                {
                    Val = ParentStep.Parent.PreLaunch
                        .FirstOrDefault(x => x.OptionType == PreLaunchOption.PreLaunchOptionEnm.SetSp)?.Value ?? 0,
                    TargetTeam = ParentStep.Parent.PartyTeam, ResType = Resource.ResourceType.SP
                });
        }
        else
        {
            ParentStep.Parent.DoEnterCombat = true;
            ParentStep.Parent.CurrentFight = null;
            ParentStep.Parent.CurrentFightStep -= 1;
        }

        base.ProcEvent(revert);
    }
}