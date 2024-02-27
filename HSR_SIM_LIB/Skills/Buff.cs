using System.Collections.Generic;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills;

public class Buff(Unit sourceUnit, Buff reference = null) : CloneClass
{
    public string CustomIconName { get; init; }
    public List<Effect> Effects { get; set; } = [];
    public Unit CarrierUnit { get; set; }
    public Buff Reference { get; set; } = reference;
    public Unit SourceUnit { get; set; } = sourceUnit;
    public int Stack { get; set; } = 1;
    //calculated stacks will overwrite Stack value
    public delegate int CalculateIntVal(Event ent);
    public CalculateIntVal CalculateStacks { get; init; }
    public int MaxStack { get; init; } = 1;
    public enum BuffType
    {
        Buff,
        Debuff,
        Dot
    }
    public BuffType Type { get; init; } = BuffType.Buff;

    public override object Clone()
    {
        var newClone = (Buff)MemberwiseClone();
        var oldEff = newClone.Effects;
        newClone.Effects = new List<Effect>();
        //clone calculated effects
        foreach (var eff in oldEff)
            if (eff.DynamicValue)
                newClone.Effects.Add((Effect)eff.Clone());
            else
                newClone.Effects.Add(eff);


        return newClone;
    }
}