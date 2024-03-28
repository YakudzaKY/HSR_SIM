using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_CLIENT.ViewModels;
/// <summary>
/// Unit view model
/// </summary>
/// <param name="unit">for this Unit vm will be created</param>
public class UnitViewModel(Unit unit)
{
    public string Name => unit.Name;

    public List<BuffViewModel> AppliedBuffs =>unit.AppliedBuffs.Select(buff => new BuffViewModel(buff)).ToList();
  
    public List<BuffViewModel> PassiveBuffs => unit.PassiveBuffs.Where(x=>x.Truly(x,null,null,null)&& !x.IsTargetCheck).Select(buff => new BuffViewModel(buff)).ToList(); 
    public List<BuffViewModel> InactivePassiveBuffs =>unit.PassiveBuffs.Where(x => !x.Truly(x, null, null, null) && !x.IsTargetCheck).Select(buff => new BuffViewModel(buff)).ToList(); 
    public List<BuffViewModel> TargetCheckBuffs => unit.PassiveBuffs.Where(x=>x.IsTargetCheck).Select(buff => new BuffViewModel(buff)).ToList();

    public record UnitStatRec(string Name,Formula StatFormula)
    {
        public double Value => StatFormula.Result;

        public IEnumerable<BuffViewModel> Buffs =>
            StatFormula.TraceBuffs().Select(buff => new BuffViewModel(buff, StatFormula, null));

    }
    //trying build like in game stat frame 
    public List<UnitStatRec> Stats
    {
        get
        {
            var res = new List<UnitStatRec>();
            res.Add(new UnitStatRec("HP",unit.MaxHp()));
            res.Add(new UnitStatRec("ATK",unit.Attack()));
            res.Add(new UnitStatRec("DEF",unit.Def()));
            res.Add(new UnitStatRec("SPD",unit.Speed()));
            res.Add(new UnitStatRec("Crit chance",unit.CritChance()));
            res.Add(new UnitStatRec("Crit dmg",unit.CritDamage()));
            res.Add(new UnitStatRec("Break dmg",unit.BreakDmg()));
            res.Add(new UnitStatRec("Outcome heal",unit.OutgoingHealMultiplier()));
            res.Add(new UnitStatRec("Income heal",unit.IncomingHealMultiplier()));
            res.Add(new UnitStatRec("Energy rate",unit.EnergyRegenPrc()));
            res.Add(new UnitStatRec("Effect hit",unit.EffectHit()));
            res.Add(new UnitStatRec("Effect res",unit.EffectRes()));
            return res;
        }
    }

}