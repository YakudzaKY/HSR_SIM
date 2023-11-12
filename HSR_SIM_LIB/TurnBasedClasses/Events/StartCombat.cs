using System;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

//starting the combat
internal class StartCombat : Event
{
    public StartCombat(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public override string GetDescription()
    {
        return "Combat was started";
    }

    public override void ProcEvent(bool revert)
    {
        //Loading combat

        if (!revert)
        {
            Parent.Parent.DoEnterCombat = false;
            Parent.Parent.CurrentFight =
                new CombatFight(Parent.Parent.CurrentScenario.Fights[Parent.Parent.CurrentFightStep], Parent.Parent);
            Parent.Parent.CurrentFightStep += 1;
            if (!TriggersHandled)
                ChildEvents.Add(new PartyResourceGain(Parent, this, null)
                {
                    Val = Constant.StartSp, TargetTeam = Parent.Parent.PartyTeam, ResType = Resource.ResourceType.SP
                });
        }
        else
        {
            Parent.Parent.DoEnterCombat = true;
            Parent.Parent.CurrentFight = null;
            Parent.Parent.CurrentFightStep -= 1;
        }

        base.ProcEvent(revert);
    }
}