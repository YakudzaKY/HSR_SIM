using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

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

    public record UnitStatRec(string StatName)
    {
        public string Name => StatName;
        public string StatDigits => StatName;
    }
    //trying build like in game stat frame 
    public List<UnitStatRec> Stats
    {
        get
        {
            var res = new List<UnitStatRec>();
            res.Add(new UnitStatRec("ATK"));
            res.Add(new UnitStatRec("DEF"));
            res.Add(new UnitStatRec("Crit chance"));
            res.Add(new UnitStatRec("Crit damage"));
            res.Add(new UnitStatRec("Speed"));
            return res;
        }
    }

}