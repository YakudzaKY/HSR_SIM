using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.TurnBasedClasses.Step;

namespace HSR_SIM_LIB.TurnBasedClasses;

public class CombatFight
{
    public CombatFight(Fight refFight, SimCls parent)
    {
        ReferenceFight = refFight;
        Parent = parent;
    }

    public SimCls Parent { get; set; }
    public short CurrentWaveCnt { get; set; } = 0;
    public Fight ReferenceFight { get; set; }

    internal Wave CurrentWave { get; set; }

    public TurnR Turn { get; set; } = null;


    public IEnumerable<Unit> AllAliveUnits
    {
        get
        {
            return Parent?.PartyTeam.Units.Where(x => x.IsAlive)
                .Concat(Parent.HostileTeam.Units.Where(x => x.IsAlive)).Concat(Parent.SpecialTeam.Units);
        }
    }

    public record TurnR
    {
        public Unit Actor { get; set; } = null;
        public StepTypeEnm? TurnStage { get; set; } = null;
    }
}