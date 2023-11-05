using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.ReadyBuffs
{
    public class BuffImprisonmentWB:Buff
    {
        public BuffImprisonmentWB(Unit caster, Buff reference = null) : base(caster, reference)
        {

                DoNotClone = true;
                Dispellable = true;
                Type = ModType.Debuff;
                BaseDuration = 1;
                Effects = new List<Effect>()
                {
                    new EffImprisonment()
                        {  CalculateValue = FighterUtils.CalculateShieldBrokeDmg },
                    new EffDelay() {  Value = 0.30 * (1 + caster.GetBreakDmg(null)) },
                    new EffReduceSpdPrc() {  Value = 0.1 }
                };

        }
    }
}
