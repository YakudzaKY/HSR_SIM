using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
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

    public record WeaknessRec(string Name,IEnumerable<BuffViewModel> Buffs)
    {
        

    }

    public List<WeaknessRec> Weakness
    {
        get
        {
            var res = new List<WeaknessRec>();
            foreach (var weakness in unit.NativeWeaknesses)
            {
                res.Add(new WeaknessRec(weakness.ToString(),[]));
            }
            //weakness impair
            foreach (var weakness in unit.GetBuffEffectsByType(typeof(EffWeaknessImpair), ent: null,
                         excludeCondition: null))
            {
                res.Add(new WeaknessRec(((EffWeaknessImpair)weakness.Value.First(x=>x is EffWeaknessImpair )).Element.ToString() , new BuffViewModel[] { new(weakness.Key)} ));
            }
               
            
            return res;
        }
    }



    public List<UnitStatRec> Resists
    {
        get
        {
            var res = new List<UnitStatRec>();
            foreach (Ability.ElementEnm elem in ((Ability.ElementEnm[]) Enum.GetValues(typeof(Ability.ElementEnm))).Where(x=>x!=Ability.ElementEnm.None) )
            {
                var resVal = unit.Resists(ent:null,elem:elem);
                if (resVal.Result != 0)
                {
                    res.Add(new UnitStatRec(elem.ToString(),resVal));
                }
            }
            return res;
        }
    }
    

    public List<UnitStatRec>  DebuffResists
    {
        get
        {
            var res = new List<UnitStatRec>();
            foreach (var debuffRes in unit.NativeDebuffResists)
            {
                var effectInst = (Effect)Activator.CreateInstance(debuffRes.Debuff)!;
                var resVal = unit.DebuffResists(ent:null, effect:effectInst);
                if (resVal.Result != 0)
                {
                    res.Add(new UnitStatRec(debuffRes.Debuff.Name,resVal));
                }
                
                
            }
            //all debuff res 
            var allDebuffRes = unit.DebuffResists(ent:null, effect:null);
            if (allDebuffRes.Result != 0)
            {
                res.Add(new UnitStatRec("All debuff res",allDebuffRes));
            }
            return res;
        }
    }
    
    
    public List<UnitStatRec>  AbilityTypeBoosters
    {
        get
        {
            var res = new List<UnitStatRec>();
            foreach (Ability.AbilityTypeEnm aType in (Ability.AbilityTypeEnm[]) Enum.GetValues(typeof(Ability.AbilityTypeEnm)))
            {
                var resVal = unit.AbilityTypeMultiplier(ent:null,abilityType:aType);
                if (resVal.Result != 0)
                {
                    res.Add(new UnitStatRec(aType.ToString(),resVal));
                }
            }
            return res;
        }
    }
}